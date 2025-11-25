using System;

namespace BMES.Core.Models
{
    public class AuditEvent
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string ActionDescription { get; set; }
        public string ValueBefore { get; set; }
        public string ValueAfter { get; set; }
    }
}
