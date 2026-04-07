using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Application.Queries
{
    public class StatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class GetOrderStatusBreakdownQuery : IRequest<List<StatusCount>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }

    public class GetOrderStatusBreakdownQueryHandler : IRequestHandler<GetOrderStatusBreakdownQuery, List<StatusCount>>
    {
        private readonly AnalyticsDbContext _dbContext;

        public GetOrderStatusBreakdownQueryHandler(AnalyticsDbContext dbContext) => _dbContext = dbContext;

        public async Task<List<StatusCount>> Handle(GetOrderStatusBreakdownQuery request, CancellationToken cancellationToken)
        {
            var from = request.From ?? DateTime.UtcNow.AddDays(-30);
            var to = request.To ?? DateTime.UtcNow;

            return await _dbContext.AnalyticsOrders
                .AsNoTracking()
                .Where(o => o.PlacedAt >= from && o.PlacedAt <= to)
                .GroupBy(o => o.Status)
                .Select(g => new StatusCount { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);
        }
    }
}
