using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using BMES.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace BMES.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly BmesDbContext _context;

        public OrderRepository(BmesDbContext context)
        {
            _context = context;
        }

        public async Task AddOrderAsync(ProductionOrder order)
        {
            await _context.ProductionOrders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(int id)
        {
            var order = await _context.ProductionOrders.FindAsync(id);
            if (order != null)
            {
                _context.ProductionOrders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductionOrder>> GetAllOrdersAsync()
        {
            return await _context.ProductionOrders.ToListAsync();
        }

        public async Task<ProductionOrder> GetOrderByIdAsync(int id)
        {
            return (await _context.ProductionOrders.FindAsync(id))!;
        }

        public async Task UpdateOrderAsync(ProductionOrder order)
        {
            _context.ProductionOrders.Update(order);
            await _context.SaveChangesAsync();
        }
    }
}
