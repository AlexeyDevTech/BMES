using BMES.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BMES.Core
{
    public class OpcUaManager : IOpcUaManager
    {
        private readonly IOpcUaClient _opcUaClient;
        private readonly ConcurrentDictionary<string, IObservable<object>> _subscriptions = new ConcurrentDictionary<string, IObservable<object>>();
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public OpcUaManager(IOpcUaClient opcUaClient)
        {
            _opcUaClient = opcUaClient;
        }

        public bool IsConnected => _opcUaClient.IsConnected;

        public Task ConnectAsync()
        {
            return _opcUaClient.ConnectAsync();
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

        public Task WriteAsync<T>(string nodeId, T value)
        {
            return _opcUaClient.WriteAsync(nodeId, value);
        }
    }
}
