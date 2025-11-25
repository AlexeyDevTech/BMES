using BMES.Contracts.Events;
using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Opc.Ua.Client;
using Polly;
using Polly.Retry;
using Prism.Events;
using System.Collections.Concurrent;
using System.Reactive.Linq;

namespace BMES.Infrastructure
{
    public class OpcUaClient : IOpcUaClient, IAsyncDisposable
    {
        private readonly string _serverUrl;
        private readonly ILogger<OpcUaClient>? _logger;
        private readonly IEventAggregator _eventAggregator;
        private Session? _session;
        private readonly SemaphoreSlim _sessionLock = new SemaphoreSlim(1, 1);
        private readonly AsyncRetryPolicy _retryPolicy;

        // Храним подписки чтобы корректно удалить при Disconnect
        private readonly ConcurrentBag<Subscription> _subscriptions = new ConcurrentBag<Subscription>();
        private bool _disposed = false;

        public OpcUaClient(ILogger<OpcUaClient>? logger, IEventAggregator eventAggregator) : this("opc.tcp://localhost:54000", logger, eventAggregator)
        {
        }

        private OpcUaClient(string serverUrl, ILogger<OpcUaClient>? logger, IEventAggregator eventAggregator)
        {
            _serverUrl = serverUrl ?? throw new ArgumentNullException(nameof(serverUrl));
            _logger = logger;
            _eventAggregator = eventAggregator;
            _retryPolicy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(5, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    (exception, timespan, context) =>
                    {
                        _logger?.LogWarning("Connection failed. Retrying in {timespan}. Error: {exception}", timespan, exception.Message);
                    });
        }
        
        public bool IsConnected => _session != null && _session.Connected;

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await _sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (IsConnected)
                {
                    _logger?.LogWarning("Already connected.");
                    return;
                }

                var config = new ApplicationConfiguration
                {
                    ApplicationName = "BMES.OpcUaClient",
                    ApplicationUri = Utils.Format($"urn:{System.Net.Dns.GetHostName()}:BMES.OpcUaClient"),
                    ApplicationType = ApplicationType.Client,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier
                        {
                            StoreType = @"Directory",
                            StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\MachineDefault",
                            SubjectName = "CN=BMES.OpcUaClient, O=BMES"
                        },
                        TrustedIssuerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Certificate Authorities" },
                        TrustedPeerCertificates = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\UA Applications" },
                        RejectedCertificateStore = new CertificateTrustList { StoreType = @"Directory", StorePath = @"%CommonApplicationData%\OPC Foundation\CertificateStores\RejectedCertificates" },
                        AutoAcceptUntrustedCertificates = true,
                        AddAppCertToTrustedStore = true
                    },
                    TransportConfigurations = new TransportConfigurationCollection(),
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                    TraceConfiguration = new TraceConfiguration()
                };

                // Валидируем конфиг (создание сертификата при необходимости)
                await config.ValidateAsync(ApplicationType.Client, cancellationToken).ConfigureAwait(false);

                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    config.CertificateValidator.CertificateValidation += (s, e) =>
                    {
                        // Авто-акцепт только для "untrusted" (как в вашем коде)
                        if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
                        {
                            e.Accept = true;
                        }
                    };
                }

                // Попробуем найти подходящий endpoint через Discovery (более надёжный вариант чем просто new EndpointDescription(url))
                EndpointDescription endpointDescription;

                try
                {
                    // 1. Получаем список endpoint’ов через дискавери
                    EndpointDescriptionCollection endpoints;
                    using (var discovery = DiscoveryClient.Create(new Uri(_serverUrl)))
                    {
                        endpoints = discovery.GetEndpoints(null);
                    }

                    // 2. Выбираем endpoint согласно параметру useSecurity (false = без шифрования)
                    endpointDescription = CoreClientUtils.SelectEndpoint(
                        config,                    // ваш ApplicationConfiguration
                        new Uri(_serverUrl),       // endpoint URL
                        endpoints,                 // найденные endpoints
                        useSecurity: false         // или true — по ситуации
                    );

                    _logger?.LogDebug("Selected endpoint: {Endpoint}", endpointDescription.EndpointUrl);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to select endpoint via discovery, using bare URL.");
                    endpointDescription = new EndpointDescription(_serverUrl);
                }

                var endpointConfiguration = EndpointConfiguration.Create(config);
                var configuredEndpoint = new ConfiguredEndpoint(null, endpointDescription, endpointConfiguration);

                // Создаём сессию корректной перегрузкой. Используем анонимную аутентификацию по умолчанию.
                // updateBeforeConnect = false, sessionName = приложение, timeout берём из конфига.
                uint sessionTimeout = (uint)Math.Max(10000, config.ClientConfiguration.DefaultSessionTimeout);

                _session = await Session.Create(
                    config,
                    configuredEndpoint,
                    false,                       // updateBeforeConnect
                    "BMES.OpcUaClient",          // sessionName
                    sessionTimeout,              // sessionTimeout (ms)
                    null,                        // userIdentity -> anonymous
                    null                         // preferredLocales
                ).ConfigureAwait(false);

                if (_session == null || !_session.Connected)
                {
                    throw new OpcUaException("Session creation succeeded but session is not connected.");
                }

                _session.KeepAlive += OnSessionKeepAlive;

                _logger?.LogInformation("Connected to OPC UA server at {ServerUrl}. SessionId: {SessionId}", _serverUrl, _session.SessionId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Connection failed.");
                throw new OpcUaException("Failed to connect to OPC UA server.", ex);
            }
            finally
            {
                _sessionLock.Release();
            }
        }
        
        private void OnSessionKeepAlive(ISession session, KeepAliveEventArgs e)
        {
            if (ServiceResult.IsBad(e.Status))
            {
                _logger?.LogWarning("OPC UA session keep-alive failed. Reconnecting...");
                _retryPolicy.ExecuteAsync(() => ConnectAsync());
            }
        }
        
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await _sessionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_session != null)
                {
                    _session.KeepAlive -= OnSessionKeepAlive;
                    try
                    {
                        // Удаляем и диспоузим все подписки, которые мы создавали
                        while (_subscriptions.TryTake(out var sub))
                        {
                            try
                            {
                                if (sub.Created)
                                {
                                    await sub.DeleteAsync(true).ConfigureAwait(false);
                                }
                            }
                            catch (Exception exSub)
                            {
                                _logger?.LogWarning(exSub, "Failed to delete subscription while disconnecting.");
                            }
                            finally
                            {
                                // попытка удалить у сессии (без исключений)
                                try { _session.RemoveSubscription(sub); } catch { }
                                try { sub.Dispose(); } catch { }
                            }
                        }

                        // Close session gracefully
                        await _session.CloseAsync(cancellationToken).ConfigureAwait(false);
                        _session.Dispose();
                        _session = null;
                        _logger?.LogInformation("Disconnected from OPC UA server.");
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Disconnection failed.");
                    }
                }
            }
            finally
            {
                _sessionLock.Release();
            }
        }

        /// <summary>
        /// Асинхронное чтение значения узла. Попробуем вернуть значение типа T.
        /// </summary>
        public async Task<T> ReadAsync<T>(string nodeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) throw new ArgumentNullException(nameof(nodeId));

            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var nodesToRead = new ReadValueIdCollection
                {
                    new ReadValueId
                    {
                        NodeId = new NodeId(nodeId),
                        AttributeId = Attributes.Value
                    }
                };

                // используем ReadAsync сессии
                var response = await _session!.ReadAsync(null, 0, TimestampsToReturn.Both, nodesToRead, cancellationToken).ConfigureAwait(false);

                if (response == null || response.Results == null || response.Results.Count == 0)
                {
                    throw new OpcUaException("Read returned no results.");
                }

                var result = response.Results[0];

                if (StatusCode.IsBad(result.StatusCode))
                {
                    throw new OpcUaException($"Read failed with status: {result.StatusCode}.");
                }

                var value = result.Value;

                if (value == null || value == DBNull.Value)
                {
                    // Для nullable типов пытаемся вернуть default
                    return default!;
                }

                // Попробуем безопасно привести типы (включая IConvertible)
                if (value is T tVal) return tVal;

                try
                {
                    var converted = (T)Convert.ChangeType(value, typeof(T));
                    return converted;
                }
                catch (Exception ex)
                {
                    // Последняя попытка: если T == string, вернуть value.ToString()
                    if (typeof(T) == typeof(string))
                    {
                        object casted = value.ToString() ?? string.Empty;
                        return (T)casted;
                    }

                    throw new OpcUaException($"Failed to convert value of node {nodeId} to {typeof(T).FullName}.", ex);
                }
            }
            catch (Exception ex) when (!(ex is OpcUaException))
            {
                _logger?.LogError(ex, "Unhandled exception during ReadAsync.");
                throw new OpcUaException("Read operation failed.", ex);
            }
        }

        /// <summary>
        /// Асинхронная запись значения.
        /// </summary>
        public async Task WriteAsync<T>(string nodeId, T value, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) throw new ArgumentNullException(nameof(nodeId));
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var nodesToWrite = new WriteValueCollection
                {
                    new WriteValue
                    {
                        NodeId = new NodeId(nodeId),
                        AttributeId = Attributes.Value,
                        Value = new DataValue(new Variant(value))
                    }
                };

                var response = await _session!.WriteAsync(null, nodesToWrite, cancellationToken).ConfigureAwait(false);

                if (response == null || response.Results == null || response.Results.Count == 0)
                {
                    throw new OpcUaException("Write returned no results.");
                }

                var result = response.Results[0];

                if (StatusCode.IsBad(result))
                {
                    throw new OpcUaException($"Write failed with status: {result}.");
                }
            }
            catch (Exception ex) when (!(ex is OpcUaException))
            {
                _logger?.LogError(ex, "Unhandled exception during WriteAsync.");
                throw new OpcUaException("Write operation failed.", ex);
            }
        }

        public void SubscribeToTag(string nodeId)
        {
            var subscription = _session.DefaultSubscription;
            var monitoredItem = new MonitoredItem(subscription.DefaultItem)
            {
                StartNodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value,
                SamplingInterval = 1000, 
            };
            monitoredItem.Notification += OnMonitoredItemNotification;
            subscription.AddItem(monitoredItem);
            subscription.ApplyChanges();
        }
        private void OnMonitoredItemNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            var value = e.NotificationValue as DataValue;
            _eventAggregator.GetEvent<TagValueChangedEvent>().Publish(new TagValue(item.StartNodeId.ToString(), value));
        }

        public void SubscribeToAlarm(string nodeId, AlarmSeverity severity)
        {
            var subscription = _session.DefaultSubscription;
            var monitoredItem = new MonitoredItem(subscription.DefaultItem)
            {
                StartNodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value,
                SamplingInterval = 1000,
                DisplayName = severity.ToString()
            };
            monitoredItem.Notification += OnAlarmNotification;
            subscription.AddItem(monitoredItem);
            subscription.ApplyChanges();
        }

        private void OnAlarmNotification(MonitoredItem item, MonitoredItemNotificationEventArgs e)
        {
            var value = e.NotificationValue as DataValue;
            if (value != null && Convert.ToBoolean(value.Value))
            {
                var alarm = new Core.Models.AlarmEvent
                {
                    NodeId = item.StartNodeId.ToString(),
                    IsActive = true,
                    IsAcknowledged = false,
                    Message = $"Alarm for {item.StartNodeId}",
                    Severity = (AlarmSeverity)Enum.Parse(typeof(AlarmSeverity), item.DisplayName),
                    Timestamp = DateTime.UtcNow
                };
                _eventAggregator.GetEvent<AlarmEventTriggered>().Publish(alarm);
            }
        }

        /// <summary>
        /// Создаёт подписку и мониторит значение nodeId. Возвращает IObservable дата-стрима значений.
        /// Асинхронная версия (чтобы не блокировать поток).
        /// </summary>
        [Obsolete("Use SubscribeToTag instead.")]
        public async Task<IObservable<DataValue>> SubscribeAsync(string nodeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nodeId)) throw new ArgumentNullException(nameof(nodeId));
            await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);

            // Создаём подписку, использую DefaultSubscription как шаблон
            var subscription = new Subscription(_session!.DefaultSubscription)
            {
                PublishingInterval = 1000,
                KeepAliveCount = 10,
                LifetimeCount = 20
            };

            var monitoredItem = new MonitoredItem(subscription.DefaultItem)
            {
                StartNodeId = new NodeId(nodeId),
                AttributeId = Attributes.Value,
                SamplingInterval = 1000,
                QueueSize = 1,
                DiscardOldest = true,
                MonitoringMode = MonitoringMode.Reporting
            };

            // Добавляем item в подписку
            subscription.AddItem(monitoredItem);
            _session.AddSubscription(subscription);

            // Асинхронно создаём подписку на сервере
            await subscription.CreateAsync(cancellationToken).ConfigureAwait(false);

            // Сохраним подписку для последующей очистки
            _subscriptions.Add(subscription);

            // Преобразуем события в IObservable<DataValue>
            var observable = Observable.FromEventPattern<MonitoredItemNotificationEventHandler, MonitoredItem, MonitoredItemNotificationEventArgs>(
                    h => monitoredItem.Notification += h,
                    h => monitoredItem.Notification -= h)
                .Select(evt => evt.EventArgs.NotificationValue as MonitoredItemNotification)
                .Where(m => m != null)
                .Select(m => m!.Value);

            _logger?.LogInformation("Created subscription for node {NodeId}. SubscriptionId: {SubId}", nodeId, subscription.Id);

            return observable;
        }

        private async Task EnsureConnectedAsync(CancellationToken cancellationToken = default)
        {
            if (!IsConnected)
            {
                await ConnectAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            try
            {
                await DisconnectAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Exception during DisposeAsync Disconnect.");
            }
            finally
            {
                _sessionLock.Dispose();
            }
        }
    }

    public class OpcUaException : Exception
    {
        public OpcUaException(string message, Exception? innerException = null) : base(message, innerException) { }
    }
}

