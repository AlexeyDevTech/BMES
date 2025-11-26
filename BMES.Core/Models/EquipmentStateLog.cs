using System;

namespace BMES.Core.Models
{
    public class EquipmentStateLog
    {
        public int Id { get; set; }
        public string? EquipmentId { get; set; }
        public string? State { get; set; }
        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
        public int ProductionOrderId { get; set; }
        public ProductionOrder? ProductionOrder { get; set; }
    }
}
