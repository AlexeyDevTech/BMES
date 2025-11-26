using System;

namespace BMES.Core.Models
{
    public class MaterialLot
    {
        public int Id { get; set; }
        public string? LotNumber { get; set; }
        public string? MaterialName { get; set; }
        public DateTime ReceivedDate { get; set; }
        public double Quantity { get; set; }
    }
}
