using System;

namespace BMES.Core.Models
{
    public abstract class WorkflowStep
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class ControlStep : WorkflowStep
    {
        public string? TagName { get; set; }
        public object? Value { get; set; }
    }

    public class MonitorStep : WorkflowStep
    {
        public string? TagName { get; set; }
        public string? Condition { get; set; } // e.g., "Value == true", "Value > 100"
    }

    public class OperatorInstructionStep : WorkflowStep
    {
        public string? InstructionText { get; set; }
    }

    public class DataCollectionStep : WorkflowStep
    {
        public string? TagName { get; set; }
        public string? DataPointName { get; set; }
    }
}
