using BMES.Contracts.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BMES.Infrastructure.Services
{
    public class ProductionReportService : IProductionReportService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IEquipmentStateLogRepository _equipmentStateLogRepository;
        private readonly IOeeCalculatorService _oeeCalculatorService;
        private readonly ILogger<ProductionReportService> _logger;

        public ProductionReportService(IOrderRepository orderRepository, IEquipmentStateLogRepository equipmentStateLogRepository, IOeeCalculatorService oeeCalculatorService, ILogger<ProductionReportService> logger)
        {
            _orderRepository = orderRepository;
            _equipmentStateLogRepository = equipmentStateLogRepository;
            _oeeCalculatorService = oeeCalculatorService;
            _logger = logger;

            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<string> GenerateShiftReport(int shiftNumber, DateTime reportDate)
        {
            try
            {
                DateTime shiftStartTime;
                DateTime shiftEndTime;

                if (shiftNumber == 1)
                {
                    shiftStartTime = reportDate.Date.AddHours(6);
                    shiftEndTime = reportDate.Date.AddHours(14);
                }
                else if (shiftNumber == 2)
                {
                    shiftStartTime = reportDate.Date.AddHours(14);
                    shiftEndTime = reportDate.Date.AddHours(22);
                }
                else if (shiftNumber == 3)
                {
                    shiftStartTime = reportDate.Date.AddHours(22);
                    shiftEndTime = reportDate.Date.AddDays(1).AddHours(6);
                }
                else
                {
                    throw new ArgumentException("Invalid shift number. Must be 1, 2, or 3.");
                }

                var orders = (await _orderRepository.GetAllOrdersAsync())
                                .Where(o => o.Status == Core.Models.OrderStatus.Completed &&
                                            o.OrderNumber != null &&
                                            o.ProductName != null &&
                                            o.MaterialLotId.HasValue &&
                                            o.ProductionOrderCompletionTime >= shiftStartTime &&
                                            o.ProductionOrderCompletionTime <= shiftEndTime)
                                .ToList();

                string equipmentId = "Mixer1"; 

                var oee = await _oeeCalculatorService.CalculateOeeAsync(equipmentId, shiftStartTime, shiftEndTime);
                var availability = await _oeeCalculatorService.CalculateAvailabilityAsync(equipmentId, shiftStartTime, shiftEndTime);
                var performance = await _oeeCalculatorService.CalculatePerformanceAsync(equipmentId, shiftStartTime, shiftEndTime);
                var quality = await _oeeCalculatorService.CalculateQualityAsync(equipmentId, shiftStartTime, shiftEndTime);

                string filePath = $"ShiftReport_Shift{shiftNumber}_{reportDate:yyyyMMdd}.pdf";

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.DefaultTextStyle(x => x.FontSize(12));

                        page.Header()
                            .Text($"Shift Report - Shift {shiftNumber} ({reportDate:dd.MM.yyyy})")
                            .SemiBold().FontSize(24).AlignCenter();

                        page.Content()
                            .PaddingVertical(1, Unit.Centimetre)
                            .Column(column =>
                            {
                                column.Spacing(20);

                                column.Item().Text($"Report Period: {shiftStartTime:HH:mm} - {shiftEndTime:HH:mm}").FontSize(14);

                                column.Item().Text("Overall Equipment Effectiveness (OEE)").SemiBold().FontSize(16);
                                column.Item().Text($"OEE: {oee:P2}").FontSize(14);
                                column.Item().Text($"Availability: {availability:P2}").FontSize(14);
                                column.Item().Text($"Performance: {performance:P2}").FontSize(14);
                                column.Item().Text($"Quality: {quality:P2}").FontSize(14);

                                column.Item().Text("Production Orders").SemiBold().FontSize(16);
                                column.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                        columns.RelativeColumn();
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().BorderBottom(1).Padding(5).Text("Order Number").SemiBold();
                                        header.Cell().BorderBottom(1).Padding(5).Text("Product Name").SemiBold();
                                        header.Cell().BorderBottom(1).Padding(5).Text("Quantity").SemiBold();
                                        header.Cell().BorderBottom(1).Padding(5).Text("Material Lot Id").SemiBold();
                                    });

                                    foreach (var order in orders)
                                    {
                                        table.Cell().BorderBottom(1).Padding(5).Text(order.OrderNumber);
                                        table.Cell().BorderBottom(1).Padding(5).Text(order.ProductName);
                                        table.Cell().BorderBottom(1).Padding(5).Text(order.Quantity.ToString());
                                        table.Cell().BorderBottom(1).Padding(5).Text(order.MaterialLotId.ToString());
                                    }
                                });
                            });

                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Page ").FontSize(10);
                                x.CurrentPageNumber().FontSize(10);
                                x.Span(" of ").FontSize(10);
                                x.TotalPages().FontSize(10);
                            });
                    });
                })
                .GeneratePdf(filePath);

                _logger.LogInformation($"Shift report '{filePath}' generated successfully.");
                return filePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating shift report.");
                return string.Empty;
            }
        }
    }
}
