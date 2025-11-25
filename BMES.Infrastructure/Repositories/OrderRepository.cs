using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using BMES.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;

namespace BMES.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly BmesDbContext _context;

        public OrderRepository(BmesDbContext context)
        {
            _context = context;
        }

        public void AddOrder(ProductionOrder order)
        {
            _context.ProductionOrders.Add(order);
            _context.SaveChanges();
        }

        public void DeleteOrder(int id)
        {
            var order = _context.ProductionOrders.Find(id);
            if (order != null)
            {
                _context.ProductionOrders.Remove(order);
                _context.SaveChanges();
            }
        }

        public IEnumerable<ProductionOrder> GetAllOrders()
        {
            return _context.ProductionOrders.ToList();
        }

        public ProductionOrder GetOrderById(int id)
        {
            return _context.ProductionOrders.Find(id);
        }

        public void UpdateOrder(ProductionOrder order)
        {
            _context.ProductionOrders.Update(order);
            _context.SaveChanges();
        }
    }
}
