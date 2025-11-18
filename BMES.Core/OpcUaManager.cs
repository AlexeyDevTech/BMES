using BMES.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace BMES.Core
{
    public class OpcUaManager : IOpcUaManager
    {
        private readonly IOpcUaClient _opcUaClient;
        private readonly ConcurrentDictionary<string, IObservable<object>> _subscriptions = new ConcurrentDictionary<string, IObservable<object>>();

        public OpcUaManager(IOpcUaClient opcUaClient)
        {
            _opcUaClient = opcUaClient;
        }

        public IObservable<T> Subscribe<T>(string nodeId)
        {
            var observable = _subscriptions.GetOrAdd(nodeId, id =>
            {
                return _opcUaClient.Subscribe(id)
                    .Select(dataValue => Convert.ChangeType(dataValue.Value, typeof(T)))
                    .Cast<object>()
                    .Publish()
                    .RefCount();
            });

            return observable.Cast<T>();
        }

        public Task WriteAsync<T>(string nodeId, T value)
        {
            return _opcUaClient.WriteAsync(nodeId, value);
        }
    }
}
