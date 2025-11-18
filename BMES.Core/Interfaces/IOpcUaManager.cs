using System;
using System.Threading.Tasks;

namespace BMES.Core.Interfaces
{
    public interface IOpcUaManager
    {
        bool IsConnected { get; }
        Task ConnectAsync();
        Task<IObservable<T>> SubscribeAsync<T>(string nodeId);
        Task WriteAsync<T>(string nodeId, T value);
    }
}
