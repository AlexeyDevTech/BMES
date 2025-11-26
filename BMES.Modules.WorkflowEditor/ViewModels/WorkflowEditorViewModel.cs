using Prism.Mvvm;
using Prism.Commands;
using System.Threading.Tasks;
using BMES.Contracts.Interfaces;
using BMES.Core.Models;

namespace BMES.Modules.WorkflowEditor.ViewModels
{
    public class WorkflowEditorViewModel : BindableBase
    {
        private readonly IProcessWorkflowRepository _workflowRepository;

        private string _workflowName;
        public string WorkflowName
        {
            get => _workflowName;
            set => SetProperty(ref _workflowName, value);
        }

        private string _workflowDescription;
        public string WorkflowDescription
        {
            get => _workflowDescription;
            set => SetProperty(ref _workflowDescription, value);
        }

        public DelegateCommand SaveWorkflowCommand { get; private set; }

        public WorkflowEditorViewModel(IProcessWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
            SaveWorkflowCommand = new DelegateCommand(async () => await ExecuteSaveWorkflowCommand(), CanSaveWorkflow);
        }

        private bool CanSaveWorkflow()
        {
            return !string.IsNullOrWhiteSpace(WorkflowName) && !string.IsNullOrWhiteSpace(WorkflowDescription);
        }

        private async Task ExecuteSaveWorkflowCommand()
        {
            var newWorkflow = new ProcessWorkflow
            {
                Name = WorkflowName,
                Description = WorkflowDescription,
                // Steps would be added here from the UI
            };
            await _workflowRepository.AddProcessWorkflowAsync(newWorkflow);
            // Optionally, clear form or show confirmation
            WorkflowName = string.Empty;
            WorkflowDescription = string.Empty;
        }
    }
}
