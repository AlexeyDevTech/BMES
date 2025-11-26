namespace BMES.Core.Models
{
    public class ProductionOrder
    {
        public int Id { get; set; }
        public string? OrderNumber { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public OrderStatus Status { get; set; }
        public int? MaterialLotId { get; set; }
        public DateTime ProductionOrderCompletionTime { get; set; }
    }

    public enum OrderStatus
    {
        New,
        InProgress,
        Completed,
        Cancelled
    }
}
