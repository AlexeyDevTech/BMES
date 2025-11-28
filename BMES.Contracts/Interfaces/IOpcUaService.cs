using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IOpcUaService
    {
        Task ConnectAsync(string serverUrl);
        Task WriteTagAsync(string nodeId, object value);
    }
}
