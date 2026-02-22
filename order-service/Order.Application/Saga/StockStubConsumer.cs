using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Order.Application.Saga
{
    public class StockStubConsumer : IConsumer<ReserveStock>
    {
        private readonly ILogger<StockStubConsumer> _logger;

        public StockStubConsumer(ILogger<StockStubConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
            _logger.LogInformation("Stock stub: reserving stock for order {OrderId}", context.Message.OrderId);

            await context.Publish(new StockReserved
            {
                OrderId = context.Message.OrderId
            });
        }
    }

    public class ReleaseStockStubConsumer : IConsumer<ReleaseStock>
    {
        private readonly ILogger<ReleaseStockStubConsumer> _logger;

        public ReleaseStockStubConsumer(ILogger<ReleaseStockStubConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<ReleaseStock> context)
        {
            _logger.LogInformation("Stock stub: releasing stock for order {OrderId}", context.Message.OrderId);
            return Task.CompletedTask;
        }
    }
}
