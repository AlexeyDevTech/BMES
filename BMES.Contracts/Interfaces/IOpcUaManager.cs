using System;
using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IOpcUaManager
    {
        bool IsConnected { get; }
        Task ConnectAsync();
        void SubscribeToTag(string nodeId);
        Task WriteAsync<T>(string nodeId, T value);
    }
}
