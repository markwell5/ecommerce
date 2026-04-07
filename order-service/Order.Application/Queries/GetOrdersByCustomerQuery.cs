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
    public class GetOrdersByCustomerQuery : IRequest<List<OrderResponse>>
    {
        public GetOrdersByCustomerQuery(string customerId)
        {
            CustomerId = customerId;
        }

        public string CustomerId { get; }
    }

    public class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, List<OrderResponse>>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetOrdersByCustomerQueryHandler(OrderDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<List<OrderResponse>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
        {
            var orders = await _dbContext.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == request.CustomerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            return orders.Select(o => _mapper.Map<OrderResponse>(o)).ToList();
        }
    }
}
