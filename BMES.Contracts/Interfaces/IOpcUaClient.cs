using BMES.Core.Models;
using Opc.Ua;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IOpcUaClient
    {
        bool IsConnected { get; }
        Task ConnectAsync(CancellationToken cancellationToken = default);
        Task DisconnectAsync(CancellationToken cancellationToken = default);
        Task WriteAsync<T>(string nodeId, T value, CancellationToken cancellationToken = default);
        Task<T> ReadAsync<T>(string nodeId, CancellationToken cancellationToken = default);
        //void SubscribeToTag(string nodeId);
        void SubscribeToAlarm(string nodeId, AlarmSeverity severity);
        Task<IObservable<DataValue>> SubscribeAsync(string nodeId, CancellationToken cancellationToken = default);
    }
}
