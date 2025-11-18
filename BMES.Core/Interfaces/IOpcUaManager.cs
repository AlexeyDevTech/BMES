using System;

namespace BMES.Core.Interfaces
{
    public interface IOpcUaManager
    {
        IObservable<T> Subscribe<T>(string nodeId);
        Task WriteAsync<T>(string nodeId, T value);
    }
}
