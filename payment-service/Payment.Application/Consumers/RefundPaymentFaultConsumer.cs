using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Payment.Application.Consumers
{
    public class RefundPaymentFaultConsumer : IConsumer<Fault<RefundPayment>>
    {
        private readonly ILogger<RefundPaymentFaultConsumer> _logger;

        public RefundPaymentFaultConsumer(ILogger<RefundPaymentFaultConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Fault<RefundPayment>> context)
        {
            var orderId = context.Message.Message.OrderId;
            var exceptions = context.Message.Exceptions;

            _logger.LogCritical(
                "Refund processing permanently failed for order {OrderId} after all retries. " +
                "Exceptions: {Exceptions}. MessageId: {MessageId}",
                orderId,
                string.Join("; ", exceptions.Select(e => $"{e.ExceptionType}: {e.Message}")),
                context.Message.FaultedMessageId);

            return Task.CompletedTask;
        }
    }
}
