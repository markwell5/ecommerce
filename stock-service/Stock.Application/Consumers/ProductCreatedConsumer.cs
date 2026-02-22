using System.Threading.Tasks;
using Ecommerce.Events.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stock.Application.Entities;

namespace Stock.Application.Consumers
{
    public class ProductCreatedConsumer : IConsumer<ProductCreated>
    {
        private readonly StockDbContext _dbContext;
        private readonly ILogger<ProductCreatedConsumer> _logger;

        public ProductCreatedConsumer(StockDbContext dbContext, ILogger<ProductCreatedConsumer> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProductCreated> context)
        {
            var productId = context.Message.Id;

            var exists = await _dbContext.StockItems.AnyAsync(s => s.ProductId == productId);
            if (exists)
            {
                _logger.LogInformation("Stock record already exists for product {ProductId}, skipping", productId);
                return;
            }

            _dbContext.StockItems.Add(new StockItem
            {
                ProductId = productId,
                AvailableQuantity = 0,
                ReservedQuantity = 0
            });

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Created stock record for product {ProductId}", productId);
        }
    }
}
