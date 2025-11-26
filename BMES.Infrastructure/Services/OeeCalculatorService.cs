using BMES.Contracts.Interfaces;
using BMES.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BMES.Infrastructure.Services
{
    public class OeeCalculatorService : IOeeCalculatorService
    {
        private readonly IEquipmentStateLogRepository _equipmentStateLogRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ILogger<OeeCalculatorService> _logger;

        public OeeCalculatorService(IEquipmentStateLogRepository equipmentStateLogRepository, IOrderRepository orderRepository, ILogger<OeeCalculatorService> logger)
        {
            _equipmentStateLogRepository = equipmentStateLogRepository;
            _orderRepository = orderRepository;
            _logger = logger;
        }

        public async Task<double> CalculateOeeAsync(string equipmentId, DateTime startDate, DateTime endDate)
        {
            double availability = await CalculateAvailabilityAsync(equipmentId, startDate, endDate);
            double performance = await CalculatePerformanceAsync(equipmentId, startDate, endDate);
            double quality = await CalculateQualityAsync(equipmentId, startDate, endDate);

            return availability * performance * quality;
        }

        public async Task<double> CalculateAvailabilityAsync(string equipmentId, DateTime startDate, DateTime endDate)
        {
            var logs = await _equipmentStateLogRepository.GetEquipmentStateLogsByEquipmentIdAsync(equipmentId, startDate, endDate);
            TimeSpan totalTime = endDate - startDate;
            TimeSpan runningTime = TimeSpan.Zero;

            foreach (var log in logs.Where(l => l.State == "Running")) // Assuming "Running" state indicates uptime
            {
                runningTime += log.Duration;
            }

            if (totalTime.TotalMinutes == 0) return 0;
            return runningTime.TotalMinutes / totalTime.TotalMinutes;
        }

        public async Task<double> CalculatePerformanceAsync(string equipmentId, DateTime startDate, DateTime endDate)
        {
            // This is a placeholder. Real performance calculation is complex.
            // It would involve actual production rate vs. ideal production rate.
            // For simplicity, let's assume a fixed performance for now or link to orders.
            var orders = await _orderRepository.GetAllOrdersAsync();
            var relevantOrders = orders.Where(o => o.ProductName == "Product A" && o.Status == OrderStatus.Completed); // Example

            double idealCycleTime = 1.0; // Assume 1 unit per minute
            double actualProduced = relevantOrders.Sum(o => o.Quantity);
            
            double operatingMinutes = (await _equipmentStateLogRepository.GetEquipmentStateLogsByEquipmentIdAsync(equipmentId, startDate, endDate))
                                      .Where(l => l.State == "Running").Sum(l => l.Duration.TotalMinutes);

            if (operatingMinutes == 0) return 0;

            double idealProduction = operatingMinutes / idealCycleTime;

            if (idealProduction == 0) return 0;

            return actualProduced / idealProduction;
        }

        public Task<double> CalculateQualityAsync(string equipmentId, DateTime startDate, DateTime endDate)
        {
            // This is a placeholder. Real quality calculation involves good units vs total units.
            // For simplicity, let's assume 100% quality for now.
            return Task.FromResult(1.0);
        }
    }
}
