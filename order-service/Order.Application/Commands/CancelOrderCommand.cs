using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Order.Application.Commands
{
    public class CancelOrderCommand : IRequest<bool>
    {
        public CancelOrderCommand(Guid orderId)
        {
            OrderId = orderId;
        }

        public Guid OrderId { get; }
    }

    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
    {
        private readonly OrderDbContext _dbContext;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public CancelOrderCommandHandler(OrderDbContext dbContext, ISendEndpointProvider sendEndpointProvider)
        {
            _dbContext = dbContext;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task<bool> Handle(CancelOrderCommand command, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == command.OrderId, cancellationToken);

            if (order == null)
                return false;

            var cancellableStates = new[] { "Placed", "ReservingStock", "Paying", "Confirmed" };
            if (Array.IndexOf(cancellableStates, order.Status) < 0)
                return false;

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:order-state-machine"));
            await endpoint.Send(new CancelOrder { OrderId = command.OrderId }, cancellationToken);
            return true;
        }
    }
}
