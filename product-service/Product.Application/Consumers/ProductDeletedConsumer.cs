using System.Threading.Tasks;
using Ecommerce.Events.Product;
using MassTransit;
using Microsoft.Extensions.Logging;
using Product.Application.Search;

namespace Product.Application.Consumers;

public class ProductDeletedConsumer : IConsumer<ProductDeleted>
{
    private readonly IProductSearchService _searchService;
    private readonly ILogger<ProductDeletedConsumer> _logger;

    public ProductDeletedConsumer(
        IProductSearchService searchService,
        ILogger<ProductDeletedConsumer> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductDeleted> context)
    {
        await _searchService.DeleteProductAsync(context.Message.Id);
        _logger.LogInformation("Removed product {ProductId} from Elasticsearch index", context.Message.Id);
    }
}
