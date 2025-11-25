using BMES.Core.Models;
using System.Collections.Generic;

namespace BMES.Contracts.Interfaces
{
    public interface IOrderRepository
    {
        ProductionOrder GetOrderById(int id);
        IEnumerable<ProductionOrder> GetAllOrders();
        void AddOrder(ProductionOrder order);
        void UpdateOrder(ProductionOrder order);
        void DeleteOrder(int id);
    }
}
