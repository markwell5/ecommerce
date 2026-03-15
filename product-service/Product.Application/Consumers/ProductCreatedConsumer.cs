using System.Threading.Tasks;
using Ecommerce.Events.Product;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Product.Application.Search;

namespace Product.Application.Consumers;

public class ProductCreatedConsumer : IConsumer<ProductCreated>
{
    private readonly ProductDbContext _dbContext;
    private readonly IProductSearchService _searchService;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(
        ProductDbContext dbContext,
        IProductSearchService searchService,
        ILogger<ProductCreatedConsumer> logger)
    {
        _dbContext = dbContext;
        _searchService = searchService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreated> context)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == context.Message.Id);

        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found for indexing", context.Message.Id);
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

        _logger.LogInformation("Indexed product {ProductId} in Elasticsearch", product.Id);
    }
}
