using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Stock.Application.Consumers
{
    public class ReserveStockFaultConsumer : IConsumer<Fault<ReserveStock>>
    {
        private readonly ILogger<ReserveStockFaultConsumer> _logger;

        public ReserveStockFaultConsumer(ILogger<ReserveStockFaultConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<Fault<ReserveStock>> context)
        {
            var orderId = context.Message.Message.OrderId;
            var exceptions = context.Message.Exceptions;

            _logger.LogCritical(
                "Stock reservation permanently failed for order {OrderId} after all retries. " +
                "Exceptions: {Exceptions}. MessageId: {MessageId}",
                orderId,
                string.Join("; ", exceptions.Select(e => $"{e.ExceptionType}: {e.Message}")),
                context.Message.FaultedMessageId);

            return Task.CompletedTask;
        }
    }
}
