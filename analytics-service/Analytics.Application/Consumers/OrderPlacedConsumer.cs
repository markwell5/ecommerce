using System;
using System.Threading.Tasks;
using Analytics.Application.Entities;
using Ecommerce.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Consumers
{
    public class OrderPlacedConsumer : IConsumer<OrderPlaced>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(AnalyticsDbContext dbContext, ILogger<OrderPlacedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderPlaced> context)
        {
            var msg = context.Message;
            var exists = await _dbContext.AnalyticsOrders.AnyAsync(o => o.OrderId == msg.OrderId);
            if (exists) return;

            _dbContext.AnalyticsOrders.Add(new AnalyticsOrder
            {
                OrderId = msg.OrderId,
                CustomerId = msg.CustomerId,
                Status = "Placed",
                TotalAmount = msg.TotalAmount,
                PlacedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Analytics: Order {OrderId} placed, amount {Amount}", msg.OrderId, msg.TotalAmount);
        }
    }
}
