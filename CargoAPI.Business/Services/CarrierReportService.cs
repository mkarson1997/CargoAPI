using CargoAPI.DataAccess;
using CargoAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CargoAPI.Business.Services
{
    public class CarrierReportService : ICarrierReportService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CarrierReportService> _logger;

        public CarrierReportService(AppDbContext context, ILogger<CarrierReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task GenerateReportsAsync()
        {
            _logger.LogInformation("Starting carrier report generation...");

            var orders = await _context.Orders.ToListAsync();

            // Group orders by carrier and date, sum costs
            var dailyTotals = orders
                .GroupBy(o => new { o.CarrierId, Date = o.OrderDate.Date })
                .Select(g => new
                {
                    g.Key.CarrierId,
                    g.Key.Date,
                    TotalCost = g.Sum(o => o.OrderCarrierCost)
                })
                .ToList();

            foreach (var dailyTotal in dailyTotals)
            {
                var existing = await _context.CarrierReports
                    .FirstOrDefaultAsync(cr => cr.CarrierId == dailyTotal.CarrierId
                                               && cr.CarrierReportDate == dailyTotal.Date);

                if (existing != null)
                {
                    existing.CarrierCost = dailyTotal.TotalCost;
                }
                else
                {
                    await _context.CarrierReports.AddAsync(new CarrierReport
                    {
                        CarrierId = dailyTotal.CarrierId,
                        CarrierCost = dailyTotal.TotalCost,
                        CarrierReportDate = dailyTotal.Date
                    });
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Carrier report generation completed. {Count} daily report rows processed.", dailyTotals.Count);
        }

        public async Task<List<CarrierReport>> GetAllReportsAsync()
        {
            return await _context.CarrierReports
                .OrderByDescending(cr => cr.CarrierReportDate)
                .ThenBy(cr => cr.CarrierId)
                .ToListAsync();
        }
    }
}
