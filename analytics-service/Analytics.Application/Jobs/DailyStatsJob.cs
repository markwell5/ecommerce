using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Analytics.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Jobs
{
    public class DailyStatsJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DailyStatsJob> _logger;

        public DailyStatsJob(IServiceScopeFactory scopeFactory, ILogger<DailyStatsJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ComputeDailyStats(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Daily stats computation failed");
                }

                // Wait until next midnight + 1 minute
                var now = DateTime.UtcNow;
                var nextRun = now.Date.AddDays(1).AddMinutes(1);
                var delay = nextRun - now;
                _logger.LogInformation("Next daily stats computation in {Delay}", delay);
                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task ComputeDailyStats(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();

            // Compute stats for yesterday (and any missing days in the last 7 days)
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            for (var i = 1; i <= 7; i++)
            {
                var date = today.AddDays(-i);
                var exists = await dbContext.DailyStats.AnyAsync(s => s.Date == date, cancellationToken);
                if (exists) continue;

                var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                var dayEnd = date.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

                var orders = dbContext.AnalyticsOrders
                    .AsNoTracking()
                    .Where(o => o.PlacedAt >= dayStart && o.PlacedAt < dayEnd);

                var orderCount = await orders.CountAsync(cancellationToken);
                if (orderCount == 0) continue;

                var revenue = await orders
                    .Where(o => o.Status != "Cancelled")
                    .SumAsync(o => o.TotalAmount - o.RefundAmount, cancellationToken);
                var cancelledCount = await orders.CountAsync(o => o.Status == "Cancelled", cancellationToken);
                var returnedCount = await orders.CountAsync(o => o.Status == "Returned", cancellationToken);
                var newCustomerCount = await dbContext.CustomerRecords
                    .CountAsync(c => c.RegisteredAt >= dayStart && c.RegisteredAt < dayEnd, cancellationToken);

                dbContext.DailyStats.Add(new DailyStat
                {
                    Date = date,
                    OrderCount = orderCount,
                    Revenue = revenue,
                    AvgOrderValue = orderCount > 0 ? revenue / orderCount : 0,
                    CancelledCount = cancelledCount,
                    ReturnedCount = returnedCount,
                    NewCustomerCount = newCustomerCount,
                    ComputedAt = DateTime.UtcNow
                });

                await dbContext.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Computed daily stats for {Date}: {OrderCount} orders, ${Revenue} revenue",
                    date, orderCount, revenue);
            }
        }
    }
}
