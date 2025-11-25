using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IOpcUaService
    {
        Task ConnectAsync(string serverUrl);
        void SubscribeToTag(string nodeId);
        Task WriteTagAsync(string nodeId, object value);
    }
}
