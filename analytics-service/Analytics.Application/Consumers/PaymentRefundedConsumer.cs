using System;
using System.Threading.Tasks;
using Ecommerce.Events.Payment;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Consumers
{
    public class PaymentRefundedConsumer : IConsumer<PaymentRefunded>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<PaymentRefundedConsumer> _logger;

        public PaymentRefundedConsumer(AnalyticsDbContext dbContext, ILogger<PaymentRefundedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentRefunded> context)
        {
            var msg = context.Message;
            var order = await _dbContext.AnalyticsOrders.FirstOrDefaultAsync(o => o.OrderId == msg.OrderId);
            if (order == null) return;
            order.RefundAmount += msg.Amount;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Analytics: Refund {Amount} for order {OrderId}", msg.Amount, msg.OrderId);
        }
    }
}
