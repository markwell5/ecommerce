using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Order.Application.Commands
{
    public class ReturnOrderCommand : IRequest<bool>
    {
        public ReturnOrderCommand(Guid orderId) { OrderId = orderId; }
        public Guid OrderId { get; }
    }

    public class ReturnOrderCommandHandler : IRequestHandler<ReturnOrderCommand, bool>
    {
        private readonly OrderDbContext _dbContext;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public ReturnOrderCommandHandler(OrderDbContext dbContext, ISendEndpointProvider sendEndpointProvider)
        {
            _dbContext = dbContext;
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task<bool> Handle(ReturnOrderCommand command, CancellationToken cancellationToken)
        {
            var order = await _dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == command.OrderId, cancellationToken);

            if (order == null || order.Status != "Delivered")
                return false;

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:order-state-machine"));
            await endpoint.Send(new ReturnOrder { OrderId = command.OrderId }, cancellationToken);
            return true;
        }
    }
}
