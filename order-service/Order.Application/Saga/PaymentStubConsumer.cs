using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Order.Application.Saga
{
    public class PaymentStubConsumer : IConsumer<ProcessPayment>
    {
        private readonly ILogger<PaymentStubConsumer> _logger;

        public PaymentStubConsumer(ILogger<PaymentStubConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessPayment> context)
        {
            _logger.LogInformation("Payment stub: processing payment of {Amount} for order {OrderId}",
                context.Message.Amount, context.Message.OrderId);

            await context.Publish(new PaymentSucceeded
            {
                OrderId = context.Message.OrderId
            });
        }
    }
}
