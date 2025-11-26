using BMES.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IProcessWorkflowRepository
    {
        Task<IEnumerable<ProcessWorkflow>> GetAllProcessWorkflowsAsync();
        Task<ProcessWorkflow> GetProcessWorkflowByIdAsync(int id);
        Task AddProcessWorkflowAsync(ProcessWorkflow workflow);
        Task UpdateProcessWorkflowAsync(ProcessWorkflow workflow);
        Task DeleteProcessWorkflowAsync(int id);
    }
}
