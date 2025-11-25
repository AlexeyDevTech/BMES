using System;

namespace BMES.Core.Models
{
    public class ProductionOrderEvent
    {
        public int Id { get; set; }
        public ProductionOrderEventType EventType { get; set; }
        public DateTime EventTimestamp { get; set; }
        public string? Description { get; set; }
    }

    public enum ProductionOrderEventType
    {
        OrderCreated,
        ProductionStarted,
        MaterialAdded,
        QuantityUpdated,
        ProductionCompleted,
        OrderCancelled,
        ErrorOccurred,
        Other
    }
}
