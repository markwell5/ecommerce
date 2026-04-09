using System.Linq;
using System.Threading.Tasks;
using Ecommerce.Events.Stock;
using Ecommerce.Events.Wishlist;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Wishlist.Application.Consumers
{
    public class StockUpdatedConsumer : IConsumer<StockUpdated>
    {
        private readonly WishlistDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<StockUpdatedConsumer> _logger;

        public StockUpdatedConsumer(WishlistDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger<StockUpdatedConsumer> logger)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockUpdated> context)
        {
            var message = context.Message;

            if (message.AvailableQuantity <= 0)
                return;

            _logger.LogInformation("Product {ProductId} is back in stock with quantity {Quantity}",
                message.ProductId, message.AvailableQuantity);

            var itemsToNotify = await _dbContext.WishlistItems
                .Include(i => i.Wishlist)
                .Where(i => i.ProductId == message.ProductId && i.NotifyOnRestock)
                .ToListAsync(context.CancellationToken);

            foreach (var item in itemsToNotify)
            {
                _logger.LogInformation("Sending back-in-stock notification for product {ProductId} to customer {CustomerId}",
                    message.ProductId, item.Wishlist.CustomerId);

                await _publishEndpoint.Publish(new BackInStockNotification
                {
                    CustomerId = item.Wishlist.CustomerId,
                    ProductId = message.ProductId,
                    WishlistId = item.WishlistId,
                    WishlistItemId = item.Id
                }, context.CancellationToken);
            }
        }
    }
}
