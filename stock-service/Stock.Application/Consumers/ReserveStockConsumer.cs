using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using Ecommerce.Events.Stock;
using Ecommerce.Model.Order.Request;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Stock.Application.Consumers
{
    public class ReserveStockConsumer : IConsumer<ReserveStock>
    {
        private readonly StockDbContext _dbContext;
        private readonly ILogger<ReserveStockConsumer> _logger;
        private readonly int _lowStockThreshold;

        public ReserveStockConsumer(StockDbContext dbContext, ILogger<ReserveStockConsumer> logger, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;
            _lowStockThreshold = configuration.GetValue("StockSettings:LowStockThreshold", 10);
        }

        public async Task Consume(ConsumeContext<ReserveStock> context)
        {
            var orderId = context.Message.OrderId;
            var items = JsonSerializer.Deserialize<List<OrderLineItem>>(context.Message.ItemsJson);

            _logger.LogInformation("Reserving stock for order {OrderId}, {ItemCount} items", orderId, items.Count);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var productIds = items.Select(i => i.ProductId).ToList();

                // Lock rows for update
                var stockItems = await _dbContext.StockItems
                    .FromSqlRaw(
                        "SELECT * FROM \"StockItems\" WHERE \"ProductId\" = ANY({0}) FOR UPDATE",
                        productIds.ToArray())
                    .ToListAsync();

                // Check all items have sufficient stock
                var insufficientItems = new List<string>();
                foreach (var item in items)
                {
                    var stock = stockItems.FirstOrDefault(s => s.ProductId == item.ProductId);
                    if (stock == null)
                    {
                        insufficientItems.Add($"Product {item.ProductId}: no stock record");
                    }
                    else if (stock.AvailableQuantity < item.Quantity)
                    {
                        insufficientItems.Add($"Product {item.ProductId}: requested {item.Quantity}, available {stock.AvailableQuantity}");
                    }
                }

                if (insufficientItems.Any())
                {
                    await transaction.RollbackAsync();
                    var reason = string.Join("; ", insufficientItems);
                    _logger.LogWarning("Stock reservation failed for order {OrderId}: {Reason}", orderId, reason);

                    await context.Publish(new StockReservationFailed
                    {
                        OrderId = orderId,
                        Reason = reason
                    });
                    return;
                }

                // Reserve stock
                var lowStockProducts = new List<(long ProductId, int Available)>();
                foreach (var item in items)
                {
                    var stock = stockItems.First(s => s.ProductId == item.ProductId);
                    stock.AvailableQuantity -= item.Quantity;
                    stock.ReservedQuantity += item.Quantity;

                    if (stock.AvailableQuantity <= _lowStockThreshold)
                    {
                        lowStockProducts.Add((stock.ProductId, stock.AvailableQuantity));
                    }
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Stock reserved for order {OrderId}", orderId);

                await context.Publish(new StockReserved { OrderId = orderId });

                // Publish low stock warnings after commit
                foreach (var (productId, available) in lowStockProducts)
                {
                    _logger.LogWarning("Low stock for product {ProductId}: {AvailableQuantity} remaining", productId, available);
                    await context.Publish(new LowStock
                    {
                        ProductId = productId,
                        AvailableQuantity = available
                    });
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
