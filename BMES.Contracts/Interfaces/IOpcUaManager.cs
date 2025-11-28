using System;
using System.Threading.Tasks;
using Opc.Ua;

namespace BMES.Contracts.Interfaces
{
    public interface IOpcUaManager
    {
        bool IsConnected { get; }
        Task ConnectAsync();
        Task<IObservable<T>> SubscribeAsync<T>(string nodeId);
        //void SubscribeToTag(string nodeId);
        Task WriteAsync<T>(string nodeId, T value);
    }
}
