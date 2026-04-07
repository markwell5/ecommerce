using System;
using System.Threading.Tasks;
using Ecommerce.Events.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Consumers
{
    public class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<OrderConfirmedConsumer> _logger;

        public OrderConfirmedConsumer(AnalyticsDbContext dbContext, ILogger<OrderConfirmedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderConfirmed> context)
        {
            var order = await _dbContext.AnalyticsOrders.FirstOrDefaultAsync(o => o.OrderId == context.Message.OrderId);
            if (order == null) return;
            order.Status = "Confirmed";
            order.ConfirmedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Analytics: Order {OrderId} confirmed", context.Message.OrderId);
        }
    }

    public class OrderShippedConsumer : IConsumer<OrderShipped>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<OrderShippedConsumer> _logger;

        public OrderShippedConsumer(AnalyticsDbContext dbContext, ILogger<OrderShippedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderShipped> context)
        {
            var order = await _dbContext.AnalyticsOrders.FirstOrDefaultAsync(o => o.OrderId == context.Message.OrderId);
            if (order == null) return;
            order.Status = "Shipped";
            order.ShippedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Analytics: Order {OrderId} shipped", context.Message.OrderId);
        }
    }

    public class OrderDeliveredConsumer : IConsumer<OrderDelivered>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<OrderDeliveredConsumer> _logger;

        public OrderDeliveredConsumer(AnalyticsDbContext dbContext, ILogger<OrderDeliveredConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderDelivered> context)
        {
            var order = await _dbContext.AnalyticsOrders.FirstOrDefaultAsync(o => o.OrderId == context.Message.OrderId);
            if (order == null) return;
            order.Status = "Delivered";
            order.DeliveredAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Analytics: Order {OrderId} delivered", context.Message.OrderId);
        }
    }

    public class OrderCancelledConsumer : IConsumer<OrderCancelled>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<OrderCancelledConsumer> _logger;

        public OrderCancelledConsumer(AnalyticsDbContext dbContext, ILogger<OrderCancelledConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCancelled> context)
        {
            var order = await _dbContext.AnalyticsOrders.FirstOrDefaultAsync(o => o.OrderId == context.Message.OrderId);
            if (order == null) return;
            order.Status = "Cancelled";
            order.CancelledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Analytics: Order {OrderId} cancelled", context.Message.OrderId);
        }
    }

    public class OrderReturnedConsumer : IConsumer<OrderReturned>
    {
        private readonly AnalyticsDbContext _dbContext;
        private readonly ILogger<OrderReturnedConsumer> _logger;

        public OrderReturnedConsumer(AnalyticsDbContext dbContext, ILogger<OrderReturnedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderReturned> context)
        {
            var order = await _dbContext.AnalyticsOrders.FirstOrDefaultAsync(o => o.OrderId == context.Message.OrderId);
            if (order == null) return;
            order.Status = "Returned";
            order.ReturnedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Analytics: Order {OrderId} returned", context.Message.OrderId);
        }
    }
}
