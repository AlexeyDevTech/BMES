using System;

namespace BMES.Core.Models
{
    public class AlarmEvent
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? NodeId { get; set; }
        public string? Message { get; set; }
        public AlarmSeverity Severity { get; set; }
        public bool IsActive { get; set; }
        public bool IsAcknowledged { get; set; }
    }

    public enum AlarmSeverity
    {
        Low,
        Medium,
        High,
        Critical
    }
}
