using System.Collections.Generic;

namespace BMES.Core.Models
{
    public class ProcessWorkflow
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public List<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
    }
}
