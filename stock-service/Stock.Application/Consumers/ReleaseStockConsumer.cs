using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using Ecommerce.Events.Stock;
using Ecommerce.Model.Order.Request;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Stock.Application.Consumers
{
    public class ReleaseStockConsumer : IConsumer<ReleaseStock>
    {
        private readonly StockDbContext _dbContext;
        private readonly ILogger<ReleaseStockConsumer> _logger;

        public ReleaseStockConsumer(StockDbContext dbContext, ILogger<ReleaseStockConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReleaseStock> context)
        {
            var orderId = context.Message.OrderId;
            var items = JsonSerializer.Deserialize<List<OrderLineItem>>(context.Message.ItemsJson);

            _logger.LogInformation("Releasing stock for order {OrderId}, {ItemCount} items", orderId, items.Count);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var productIds = items.Select(i => i.ProductId).ToList();

                var stockItems = await _dbContext.StockItems
                    .FromSqlRaw(
                        "SELECT * FROM \"StockItems\" WHERE \"ProductId\" = ANY({0}) FOR UPDATE",
                        productIds.ToArray())
                    .ToListAsync();

                foreach (var item in items)
                {
                    var stock = stockItems.FirstOrDefault(s => s.ProductId == item.ProductId);
                    if (stock == null) continue;

                    stock.ReservedQuantity -= item.Quantity;
                    if (stock.ReservedQuantity < 0) stock.ReservedQuantity = 0;
                    stock.AvailableQuantity += item.Quantity;
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Stock released for order {OrderId}", orderId);

                await context.Publish(new StockReleased { OrderId = orderId });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
