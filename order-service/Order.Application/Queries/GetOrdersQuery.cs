using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Order.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Order.Application.Queries
{
    public class GetOrdersQuery : IRequest<GetOrdersResult>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string Status { get; set; } = string.Empty;
    }

    public class GetOrdersResult
    {
        public List<OrderResponse> Orders { get; set; } = [];
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, GetOrdersResult>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetOrdersQueryHandler(OrderDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            var query = _dbContext.Orders.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(o => o.Status == request.Status);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new GetOrdersResult
            {
                Orders = orders.Select(o => _mapper.Map<OrderResponse>(o)).ToList(),
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
