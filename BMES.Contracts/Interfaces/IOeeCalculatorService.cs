using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IOeeCalculatorService
    {
        Task<double> CalculateOeeAsync(string equipmentId, System.DateTime startDate, System.DateTime endDate);
        Task<double> CalculateAvailabilityAsync(string equipmentId, System.DateTime startDate, System.DateTime endDate);
        Task<double> CalculatePerformanceAsync(string equipmentId, System.DateTime startDate, System.DateTime endDate);
        Task<double> CalculateQualityAsync(string equipmentId, System.DateTime startDate, System.DateTime endDate);
    }
}
