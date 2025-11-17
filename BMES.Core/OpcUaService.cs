using BMES.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;

namespace BMES.Core
{
    public class OpcUaService : IOpcUaService, IAsyncDisposable
    {
        private const string MidTempNodeId = "ns=2;s=Demo.MidTemp";
        private const string TempOutNodeId = "ns=2;s=Demo.TempOut";
        private const string HudimityNodeId = "ns=2;s=Demo.Hudimity";

        private readonly ILogger<OpcUaService> _logger;
        private ClientSessionChannel? _channel;
        private IDisposable? _subscription;

        private float _midTemp;
        public float MidTemp
        {
            get => _midTemp;
            private set => SetProperty(ref _midTemp, value);
        }

        private float _tempOut;
        public float TempOut
        {
            get => _tempOut;
            private set => SetProperty(ref _tempOut, value);
        }

        private float _hudimity;
        public float Hudimity
        {
            get => _hudimity;
            private set => SetProperty(ref _hudimity, value);
        }

        public OpcUaService(ILogger<OpcUaService> logger)
        {
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var clientDescription = new ApplicationDescription
                {
                    ApplicationName = "BMES.OpcUaClient",
                    ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:BMES.OpcUaClient",
                    ApplicationType = ApplicationType.Client
                };

                _channel = new ClientSessionChannel(
                    clientDescription,
                    null,
                    new AnonymousIdentity(),
                    "opc.tcp://localhost:54000",
                    SecurityPolicyUris.None);

                await _channel.OpenAsync(cancellationToken);
                _logger.LogInformation("Connected to OPC UA server.");

                var subscriptionRequest = new CreateSubscriptionRequest
                {
                    RequestedPublishingInterval = 1000,
                    RequestedMaxKeepAliveCount = 10,
                    RequestedLifetimeCount = 30,
                    PublishingEnabled = true,
                };

                var subscriptionResponse = await _channel.CreateSubscriptionAsync(subscriptionRequest, cancellationToken);
                var subscriptionId = subscriptionResponse.SubscriptionId;

                var itemsToCreate = new MonitoredItemCreateRequest[]
                {
                    new MonitoredItemCreateRequest { ItemToMonitor = new ReadValueId { NodeId = NodeId.Parse(MidTempNodeId), AttributeId = AttributeIds.Value }, MonitoringMode = MonitoringMode.Reporting, RequestedParameters = new MonitoringParameters { ClientHandle = 1, SamplingInterval = 1000, QueueSize = 1, DiscardOldest = true } },
                    new MonitoredItemCreateRequest { ItemToMonitor = new ReadValueId { NodeId = NodeId.Parse(TempOutNodeId), AttributeId = AttributeIds.Value }, MonitoringMode = MonitoringMode.Reporting, RequestedParameters = new MonitoringParameters { ClientHandle = 2, SamplingInterval = 1000, QueueSize = 1, DiscardOldest = true } },
                    new MonitoredItemCreateRequest { ItemToMonitor = new ReadValueId { NodeId = NodeId.Parse(HudimityNodeId), AttributeId = AttributeIds.Value }, MonitoringMode = MonitoringMode.Reporting, RequestedParameters = new MonitoringParameters { ClientHandle = 3, SamplingInterval = 1000, QueueSize = 1, DiscardOldest = true } },
                };

                var itemsRequest = new CreateMonitoredItemsRequest { SubscriptionId = subscriptionId, ItemsToCreate = itemsToCreate };
                await _channel.CreateMonitoredItemsAsync(itemsRequest, cancellationToken);

                _subscription = _channel
                    .Where(pr => pr.SubscriptionId == subscriptionId)
                    .SelectMany(pr => pr.NotificationMessage.NotificationData)
                    .Cast<DataChangeNotification>()
                    .SelectMany(notification => notification.MonitoredItems)
                    .Subscribe(OnMonitoredItemNotification);

                _logger.LogInformation("OPC UA service started and subscriptions created.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start OPC UA service.");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            _subscription?.Dispose();
            _subscription = null;

            if (_channel != null && _channel.State != CommunicationState.Closed)
            {
                await _channel.CloseAsync(cancellationToken);
                _logger.LogInformation("Disconnected from OPC UA server.");
            }
        }

        private void OnMonitoredItemNotification(MonitoredItemNotification notification)
        {
            try
            {
                if (notification.Value == null) return;

                switch (notification.ClientHandle)
                {
                    case 1:
                        MidTemp = (float)Convert.ChangeType(notification.Value.Value, typeof(float));
                        _logger.LogTrace("MidTemp updated: {MidTemp}", MidTemp);
                        break;
                    case 2:
                        TempOut = (float)Convert.ChangeType(notification.Value.Value, typeof(float));
                        _logger.LogTrace("TempOut updated: {TempOut}", TempOut);
                        break;
                    case 3:
                        Hudimity = (float)Convert.ChangeType(notification.Value.Value, typeof(float));
                        _logger.LogTrace("Hudimity updated: {Hudimity}", Hudimity);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OPC UA notification.");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}
