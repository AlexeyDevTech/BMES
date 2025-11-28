using BMES.Contracts.Events;
using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using Microsoft.Extensions.Logging;
using Opc.Ua;
using Prism.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BMES.Infrastructure
{
    public class OpcUaManager : IOpcUaManager, IOpcUaService
    {
        private readonly IOpcUaClient _opcUaClient;
        private readonly ILogger<OpcUaManager> _logger;
        private readonly IEquipmentStateLogRepository _equipmentStateLogRepository;
        private readonly ITagConfigurationService _tagConfigurationService;
        private readonly IOrderRepository _orderRepository;
        private readonly IEventAggregator _eventAggregator;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly ConcurrentDictionary<string, IObservable<object>> _subscriptions = new ConcurrentDictionary<string, IObservable<object>>();

        private Dictionary<string, (string State, DateTime Timestamp)> _equipmentLastKnownState = new Dictionary<string, (string State, DateTime Timestamp)>();

        public OpcUaManager(IOpcUaClient opcUaClient, ILogger<OpcUaManager> logger, IEquipmentStateLogRepository equipmentStateLogRepository, ITagConfigurationService tagConfigurationService, IOrderRepository orderRepository, IEventAggregator eventAggregator)
        {
            _opcUaClient = opcUaClient;
            _logger = logger;
            _equipmentStateLogRepository = equipmentStateLogRepository;
            _tagConfigurationService = tagConfigurationService;
            _orderRepository = orderRepository;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<TagValueChangedEvent>().Subscribe(OnTagValueChanged);
        }

        public bool IsConnected => _opcUaClient.IsConnected;

        async Task IOpcUaManager.ConnectAsync()
        {
            await _opcUaClient.ConnectAsync();
        }

        Task IOpcUaService.ConnectAsync(string serverUrl)
        {
            return _opcUaClient.ConnectAsync();
        }

        //public void SubscribeToTag(string nodeId)
        //{
        //    _opcUaClient.SubscribeToTag(nodeId);
        //}

        private async void OnTagValueChanged(TagValue tagValue)
        {
            string equipmentId = GetEquipmentIdFromTag(tagValue.NodeId);
            string equipmentStatusTagName = GetEquipmentStatusTagName(equipmentId);

            if (equipmentId != null && tagValue.NodeId == _tagConfigurationService.GetNodeId(equipmentStatusTagName))
            {
                string currentState = tagValue.Value.ToString();
                DateTime currentTimestamp = DateTime.UtcNow;

                if (!_equipmentLastKnownState.ContainsKey(equipmentId))
                {
                    _equipmentLastKnownState[equipmentId] = (currentState, currentTimestamp);
                    _logger.LogInformation($"Initial state for {equipmentId}: {currentState}");
                    await _equipmentStateLogRepository.AddEquipmentStateLogAsync(new EquipmentStateLog
                    {
                        EquipmentId = equipmentId,
                        State = currentState,
                        Timestamp = currentTimestamp,
                        Duration = TimeSpan.Zero,
                        ProductionOrderId = GetCurrentProductionOrderId(equipmentId)
                    });
                    return;
                }

                var (lastState, lastTimestamp) = _equipmentLastKnownState[equipmentId];

                if (lastState != currentState)
                {
                    TimeSpan duration = currentTimestamp - lastTimestamp;
                    _logger.LogInformation($"Equipment {equipmentId} changed from {lastState} to {currentState}. Duration: {duration}");

                    await _equipmentStateLogRepository.AddEquipmentStateLogAsync(new EquipmentStateLog
                    {
                        EquipmentId = equipmentId,
                        State = lastState,
                        Timestamp = lastTimestamp,
                        Duration = duration,
                        ProductionOrderId = GetCurrentProductionOrderId(equipmentId)
                    });

                    _equipmentLastKnownState[equipmentId] = (currentState, currentTimestamp);
                }
            }
        }

        private string? GetEquipmentIdFromTag(string nodeId)
        {
            if (nodeId.Contains("Mixer1")) return "Mixer1";
            return null;
        }

        private string? GetEquipmentStatusTagName(string equipmentId)
        {
            if (equipmentId == "Mixer1") return "Mixer1.Status";
            return null;
        }

        private int GetCurrentProductionOrderId(string equipmentId)
        {
            return 1;
        }

        async Task IOpcUaManager.WriteAsync<T>(string nodeId, T value)
        {
            await _opcUaClient.WriteAsync(nodeId, value);
        }
        public async Task<IObservable<T>> SubscribeAsync<T>(string nodeId)
        {
            if (_subscriptions.TryGetValue(nodeId, out var observable))
            {
                return observable.Cast<T>();
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_subscriptions.TryGetValue(nodeId, out observable))
                {
                    return observable.Cast<T>();
                }

                var newObservable = (await _opcUaClient.SubscribeAsync(nodeId))
                    .Select(dataValue => Convert.ChangeType(dataValue.Value, typeof(T)))
                    .Cast<object>()
                    .Publish()
                    .RefCount();

                _subscriptions.TryAdd(nodeId, newObservable);

                return newObservable.Cast<T>();
            }
            finally
            {
                _semaphore.Release();
            }
        }
        public async Task WriteTagAsync(string nodeId, object value)
        {
            if (value is bool boolValue)
            {
                await _opcUaClient.WriteAsync(nodeId, boolValue);
            }
            else if (value is int intValue)
            {
                await _opcUaClient.WriteAsync(nodeId, intValue);
            }
            else if (value is double doubleValue)
            {
                await _opcUaClient.WriteAsync(nodeId, doubleValue);
            }
            else if (value is string stringValue)
            {
                await _opcUaClient.WriteAsync(nodeId, stringValue);
            }
            else
            {
                throw new ArgumentException($"Unsupported type for OPC UA write: {value.GetType()}");
            }
        }
    }
}
