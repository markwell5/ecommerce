using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audit.Application.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Audit.Application.Queries
{
    public class AuditSearchResult
    {
        public List<AuditEntry> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class SearchAuditEntriesQuery : IRequest<AuditSearchResult>
    {
        public string ActorId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SearchAuditEntriesQueryHandler : IRequestHandler<SearchAuditEntriesQuery, AuditSearchResult>
    {
        private readonly AuditDbContext _dbContext;

        public SearchAuditEntriesQueryHandler(AuditDbContext dbContext) => _dbContext = dbContext;

        public async Task<AuditSearchResult> Handle(SearchAuditEntriesQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.AuditEntries.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.ActorId))
                query = query.Where(e => e.ActorId == request.ActorId);
            if (!string.IsNullOrWhiteSpace(request.EntityType))
                query = query.Where(e => e.EntityType == request.EntityType);
            if (!string.IsNullOrWhiteSpace(request.EntityId))
                query = query.Where(e => e.EntityId == request.EntityId);
            if (!string.IsNullOrWhiteSpace(request.Service))
                query = query.Where(e => e.Service == request.Service);
            if (!string.IsNullOrWhiteSpace(request.Action))
                query = query.Where(e => e.Action == request.Action);
            if (!string.IsNullOrWhiteSpace(request.CorrelationId))
                query = query.Where(e => e.CorrelationId == request.CorrelationId);
            if (request.From.HasValue)
                query = query.Where(e => e.Timestamp >= request.From.Value);
            if (request.To.HasValue)
                query = query.Where(e => e.Timestamp <= request.To.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(e => e.Timestamp)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new AuditSearchResult
            {
                Items = items,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
