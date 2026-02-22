using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Order.Application.Commands
{
    public class ShipOrderCommand : IRequest<bool>
    {
        public ShipOrderCommand(Guid orderId) { OrderId = orderId; }
        public Guid OrderId { get; }
    }

    public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, bool>
    {
        private readonly OrderDbContext _dbContext;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public ShipOrderCommandHandler(OrderDbContext dbContext, ISendEndpointProvider sendEndpointProvider)
        {
            _dbContext = dbContext;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task<bool> Handle(ShipOrderCommand command, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == command.OrderId, cancellationToken);

            if (order == null || order.Status != "Confirmed")
                return false;

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:order-state-machine"));
            await endpoint.Send(new ShipOrder { OrderId = command.OrderId }, cancellationToken);
            return true;
        }
    }
}
