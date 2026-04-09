using System;
using System.Threading.Tasks;
using Ecommerce.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Subscription.Application.Consumers
{
    public class PaymentFailedConsumer : IConsumer<OrderPaymentFailed>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly ILogger<PaymentFailedConsumer> _logger;
        private const int MaxRetries = 3;

        public PaymentFailedConsumer(SubscriptionDbContext dbContext, ILogger<PaymentFailedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPaymentFailed> context)
        {
            var msg = context.Message;
            var orderId = msg.OrderId.ToString();

            // Find subscription associated with this order via renewal history
            var renewal = await _dbContext.RenewalHistories
                .Include(r => r.Subscription)
                .FirstOrDefaultAsync(r => r.OrderId == orderId);

            if (renewal?.Subscription == null)
                return;

            var subscription = renewal.Subscription;
            subscription.FailureCount++;
            renewal.Status = "Failed";
            renewal.FailureReason = msg.Reason ?? "Payment failed";

            if (subscription.FailureCount >= MaxRetries)
            {
                subscription.Status = "Paused";
                _logger.LogWarning("Subscription {SubscriptionId} paused after {Count} payment failures",
                    subscription.Id, subscription.FailureCount);
            }
            else
            {
                // Retry with exponential backoff
                var backoffDays = Math.Pow(2, subscription.FailureCount);
                subscription.NextRenewalAt = DateTime.UtcNow.AddDays(backoffDays);
                _logger.LogInformation("Subscription {SubscriptionId} renewal retry scheduled for {NextRenewal}",
                    subscription.Id, subscription.NextRenewalAt);
            }

            subscription.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }
    }
}
