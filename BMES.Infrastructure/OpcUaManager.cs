using BMES.Contracts.Interfaces;
using System;
using System.Threading.Tasks;
using Unity;

namespace BMES.Infrastructure
{
    public class OpcUaManager : IOpcUaManager
    {
        private readonly IOpcUaClient _opcUaClient;

        public OpcUaManager(IUnityContainer container)
        {
            _opcUaClient = container.Resolve<IOpcUaClient>();
        }

        public bool IsConnected => _opcUaClient.IsConnected;

        public Task ConnectAsync()
        {
            return _opcUaClient.ConnectAsync();
        }

        public void SubscribeToTag(string nodeId)
        {
            _opcUaClient.SubscribeToTag(nodeId);
        }

        public Task WriteAsync<T>(string nodeId, T value)
        {
            return _opcUaClient.WriteAsync(nodeId, value);
        }
    }
}
