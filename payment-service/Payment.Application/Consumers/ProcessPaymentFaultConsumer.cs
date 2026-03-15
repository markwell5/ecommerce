using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Payment.Application.Consumers
{
    public class ProcessPaymentFaultConsumer : IConsumer<Fault<ProcessPayment>>
    {
        private readonly ILogger<ProcessPaymentFaultConsumer> _logger;

        public ProcessPaymentFaultConsumer(ILogger<ProcessPaymentFaultConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Fault<ProcessPayment>> context)
        {
            var orderId = context.Message.Message.OrderId;
            var exceptions = context.Message.Exceptions;

            _logger.LogCritical(
                "Payment processing permanently failed for order {OrderId} after all retries. " +
                "Exceptions: {Exceptions}. MessageId: {MessageId}",
                orderId,
                string.Join("; ", exceptions.Select(e => $"{e.ExceptionType}: {e.Message}")),
                context.Message.FaultedMessageId);

            return Task.CompletedTask;
        }
    }
}
