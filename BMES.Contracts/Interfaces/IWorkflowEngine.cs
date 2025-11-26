using BMES.Core.Models;
using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IWorkflowEngine
    {
        Task StartWorkflow(ProcessWorkflow workflow, ProductionOrder order);
        void PauseWorkflow(ProductionOrder order);
        void CancelWorkflow(ProductionOrder order);
        // Event for UI updates (e.g., WorkflowStatusUpdatedEvent) might be added later
    }
}
