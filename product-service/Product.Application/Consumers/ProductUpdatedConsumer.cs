using System.Threading.Tasks;
using Ecommerce.Events.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Product.Application.Search;

namespace Product.Application.Consumers;

public class ProductUpdatedConsumer : IConsumer<ProductUpdated>
{
    private readonly ProductDbContext _dbContext;
    private readonly IProductSearchService _searchService;
    private readonly ILogger<ProductUpdatedConsumer> _logger;

    public ProductUpdatedConsumer(
        ProductDbContext dbContext,
        IProductSearchService searchService,
        ILogger<ProductUpdatedConsumer> logger)
    {
        _dbContext = dbContext;
        _searchService = searchService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductUpdated> context)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == context.Message.Id);

        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found for re-indexing", context.Message.Id);
            return;
        }

        await _searchService.IndexProductAsync(new ProductSearchDocument
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price
        });

        _logger.LogInformation("Re-indexed product {ProductId} in Elasticsearch", product.Id);
    }
}
