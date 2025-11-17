using System.ComponentModel;

namespace BMES.Core.Interfaces
{
    public interface IOpcUaService : INotifyPropertyChanged
    {
        float MidTemp { get; }
        float TempOut { get; }
        float Hudimity { get; }

        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }
}
