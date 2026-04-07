using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Queries
{
    public class SalesOverviewResult
    {
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal AvgOrderValue { get; set; }
        public int CancelledCount { get; set; }
        public int ReturnedCount { get; set; }
        public int NewCustomerCount { get; set; }
    }

    public class GetSalesOverviewQuery : IRequest<SalesOverviewResult>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class GetSalesOverviewQueryHandler : IRequestHandler<GetSalesOverviewQuery, SalesOverviewResult>
    {
        private readonly AnalyticsDbContext _dbContext;

        public GetSalesOverviewQueryHandler(AnalyticsDbContext dbContext) => _dbContext = dbContext;

        public async Task<SalesOverviewResult> Handle(GetSalesOverviewQuery request, CancellationToken cancellationToken)
        {
            var from = request.From ?? DateTime.UtcNow.AddDays(-30);
            var to = request.To ?? DateTime.UtcNow;

            var orders = _dbContext.AnalyticsOrders
                .AsNoTracking()
                .Where(o => o.PlacedAt >= from && o.PlacedAt <= to);

            var totalRevenue = await orders
                .Where(o => o.Status != "Cancelled")
                .SumAsync(o => o.TotalAmount - o.RefundAmount, cancellationToken);
            var orderCount = await orders.CountAsync(cancellationToken);
            var cancelledCount = await orders.CountAsync(o => o.Status == "Cancelled", cancellationToken);
            var returnedCount = await orders.CountAsync(o => o.Status == "Returned", cancellationToken);

            var newCustomerCount = await _dbContext.CustomerRecords
                .AsNoTracking()
                .CountAsync(c => c.RegisteredAt >= from && c.RegisteredAt <= to, cancellationToken);

            return new SalesOverviewResult
            {
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                AvgOrderValue = orderCount > 0 ? totalRevenue / orderCount : 0,
                CancelledCount = cancelledCount,
                ReturnedCount = returnedCount,
                NewCustomerCount = newCustomerCount
            };
        }
    }
}
