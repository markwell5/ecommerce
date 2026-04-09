using System.Threading.Tasks;
using Ecommerce.Events.Payment;
using GiftCard.Application.Commands;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GiftCard.Application.Consumers
{
    public class PaymentCompletedConsumer : IConsumer<PaymentCompleted>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentCompletedConsumer> _logger;

        public PaymentCompletedConsumer(IMediator mediator, ILogger<PaymentCompletedConsumer> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentCompleted> context)
        {
            var message = context.Message;

            _logger.LogInformation("Payment completed for order {OrderId} by customer {CustomerId}, amount {Amount}",
                message.OrderId, message.CustomerId, message.Amount);

            // Gift card activation is handled at purchase time via the PurchaseGiftCardCommand.
            // This consumer can be extended to handle gift-card-as-payment-method flows.
        }
    }
}
