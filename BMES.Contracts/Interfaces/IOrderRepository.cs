using BMES.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BMES.Contracts.Interfaces
{
    public interface IOrderRepository
    {
        Task<ProductionOrder> GetOrderByIdAsync(int id);
        Task<IEnumerable<ProductionOrder>> GetAllOrdersAsync();
        Task AddOrderAsync(ProductionOrder order);
        Task UpdateOrderAsync(ProductionOrder order);
        Task DeleteOrderAsync(int id);
    }
}
