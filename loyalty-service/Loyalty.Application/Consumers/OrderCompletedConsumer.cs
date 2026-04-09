using System.Threading.Tasks;
using Ecommerce.Events.Order;
using Loyalty.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Loyalty.Application.Consumers
{
    public class OrderCompletedConsumer : IConsumer<OrderCompleted>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCompletedConsumer> _logger;

        private const int PointsPerCurrencyUnit = 10;

        public OrderCompletedConsumer(IMediator mediator, ILogger<OrderCompletedConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCompleted> context)
        {
            var message = context.Message;
            var points = (int)(message.TotalAmount * PointsPerCurrencyUnit);

            _logger.LogInformation("Crediting {Points} points to customer {CustomerId} for order {OrderId}",
                points, message.CustomerId, message.OrderId);

            await _mediator.Send(new CreditPointsCommand
            {
                CustomerId = message.CustomerId,
                Points = points,
                Description = $"Points earned from order {message.OrderId}",
                OrderId = message.OrderId.ToString()
            }, context.CancellationToken);
        }
    }
}
