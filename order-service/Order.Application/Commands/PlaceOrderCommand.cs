using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using Ecommerce.Model.Order.Request;
using Ecommerce.Model.Order.Response;
using MassTransit;
using MediatR;

namespace Order.Application.Commands
{
    public class PlaceOrderCommand : IRequest<OrderResponse>
    {
        public PlaceOrderCommand(PlaceOrderRequest request)
        {
            Request = request;
        }

        public PlaceOrderRequest Request { get; }
    }

    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderResponse>
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public PlaceOrderCommandHandler(ISendEndpointProvider sendEndpointProvider)
        {
            _sendEndpointProvider = sendEndpointProvider;
        }

        public async Task<OrderResponse> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var orderId = Guid.NewGuid();
            var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            var itemsJson = JsonSerializer.Serialize(request.Items);

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:order-state-machine"));

            await endpoint.Send(new PlaceOrder
            {
                OrderId = orderId,
                CustomerId = request.CustomerId,
                TotalAmount = totalAmount,
                ItemsJson = itemsJson
            }, cancellationToken);

            return new OrderResponse
            {
                OrderId = orderId,
                CustomerId = request.CustomerId,
                Status = "Placed",
                TotalAmount = totalAmount,
                ItemsJson = itemsJson,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
