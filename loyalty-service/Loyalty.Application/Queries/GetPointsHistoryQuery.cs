using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Loyalty.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Loyalty.Application.Queries
{
    public record GetPointsHistoryQuery(string CustomerId, int Page, int PageSize) : IRequest<PointsHistoryResponse>;

    public class GetPointsHistoryQueryHandler : IRequestHandler<GetPointsHistoryQuery, PointsHistoryResponse>
    {
        private readonly LoyaltyDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetPointsHistoryQueryHandler(LoyaltyDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<PointsHistoryResponse> Handle(GetPointsHistoryQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.PointsTransactions
                .AsNoTracking()
                .Where(t => t.CustomerId == request.CustomerId);

            var totalCount = await query.CountAsync(cancellationToken);

            var transactions = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PointsHistoryResponse
            {
                Items = _mapper.Map<List<PointsTransactionResponse>>(transactions),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
