using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Queries
{
    public class DailyRevenuePoint
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class GetDailyRevenueQuery : IRequest<List<DailyRevenuePoint>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class GetDailyRevenueQueryHandler : IRequestHandler<GetDailyRevenueQuery, List<DailyRevenuePoint>>
    {
        private readonly AnalyticsDbContext _dbContext;

        public GetDailyRevenueQueryHandler(AnalyticsDbContext dbContext) => _dbContext = dbContext;

        public async Task<List<DailyRevenuePoint>> Handle(GetDailyRevenueQuery request, CancellationToken cancellationToken)
        {
            var from = request.From ?? DateTime.UtcNow.AddDays(-30);
            var to = request.To ?? DateTime.UtcNow;

            // Try DailyStats first (pre-computed)
            var fromDate = DateOnly.FromDateTime(from);
            var toDate = DateOnly.FromDateTime(to);

            var stats = await _dbContext.DailyStats
                .AsNoTracking()
                .Where(s => s.Date >= fromDate && s.Date <= toDate)
                .OrderBy(s => s.Date)
                .ToListAsync(cancellationToken);

            if (stats.Count > 0)
            {
                return stats.Select(s => new DailyRevenuePoint
                {
                    Date = s.Date.ToString("yyyy-MM-dd"),
                    Revenue = s.Revenue,
                    OrderCount = s.OrderCount
                }).ToList();
            }

            // Fallback: compute from raw orders
            return await _dbContext.AnalyticsOrders
                .AsNoTracking()
                .Where(o => o.PlacedAt >= from && o.PlacedAt <= to && o.Status != "Cancelled")
                .GroupBy(o => o.PlacedAt.Date)
                .Select(g => new DailyRevenuePoint
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(o => o.TotalAmount - o.RefundAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(r => r.Date)
                .ToListAsync(cancellationToken);
        }
    }
}
