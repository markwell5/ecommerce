using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Order.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Order.Application.Queries
{
    public class GetOrderQuery : IRequest<OrderResponse>
    {
        public GetOrderQuery(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; }
    }

    public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderResponse>
    {
        private readonly OrderDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetOrderQueryHandler(OrderDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<OrderResponse> Handle(GetOrderQuery request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == request.OrderId, cancellationToken);

            if (order == null)
                return null;

            var response = _mapper.Map<OrderResponse>(order);

            var events = await _dbContext.OrderEvents
                .AsNoTracking()
                .Where(e => e.OrderId == request.OrderId)
                .OrderBy(e => e.OccurredAt)
                .ToListAsync(cancellationToken);

            response.Events = events.Select(e => _mapper.Map<OrderEventResponse>(e)).ToList();

            return response;
        }
    }
}
