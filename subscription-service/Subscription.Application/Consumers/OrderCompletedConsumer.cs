using System.Threading.Tasks;
using Ecommerce.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Subscription.Application.Consumers
{
    public class OrderCompletedConsumer : IConsumer<OrderCompleted>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly ILogger<OrderCompletedConsumer> _logger;

        public OrderCompletedConsumer(SubscriptionDbContext dbContext, ILogger<OrderCompletedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCompleted> context)
        {
            var msg = context.Message;
            var orderId = msg.OrderId.ToString();

            var renewal = await _dbContext.RenewalHistories
                .Include(r => r.Subscription)
                .FirstOrDefaultAsync(r => r.OrderId == orderId);

            if (renewal?.Subscription == null)
                return;

            renewal.Status = "Success";
            renewal.Subscription.FailureCount = 0;
            renewal.Subscription.UpdatedAt = System.DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Subscription {SubscriptionId} renewal confirmed for order {OrderId}",
                renewal.SubscriptionId, orderId);
        }
    }
}
