namespace BMES.Core.Interfaces
{
    public interface IOpcUaClient
    {
        Task WriteAsync<T>(string nodeId, T value, CancellationToken cancellationToken = default);
        Task<T> ReadAsync<T>(string nodeId, CancellationToken cancellationToken = default);
    }
}
