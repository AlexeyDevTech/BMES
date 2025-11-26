using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IProductionReportService
    {
        Task<string> GenerateShiftReport(int shiftNumber, System.DateTime reportDate);
    }
}
