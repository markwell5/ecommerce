using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Product.Application.Search;

namespace Product.Application.Commands;

public record ReindexProductsCommand : IRequest<int>;

public class ReindexProductsCommandHandler : IRequestHandler<ReindexProductsCommand, int>
{
    private readonly ProductDbContext _dbContext;
    private readonly IProductSearchService _searchService;
    private readonly ILogger<ReindexProductsCommandHandler> _logger;

    public ReindexProductsCommandHandler(
        ProductDbContext dbContext,
        IProductSearchService searchService,
        ILogger<ReindexProductsCommandHandler> logger)
    {
        _dbContext = dbContext;
        _searchService = searchService;
        _logger = logger;
    }

    public async Task<int> Handle(ReindexProductsCommand request, CancellationToken cancellationToken)
    {
        const int batchSize = 1000;
        var totalIndexed = 0;
        var lastId = 0L;

        while (true)
        {
            var products = await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.Id > lastId)
                .OrderBy(p => p.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);

            if (products.Count == 0)
                break;

            var documents = products.Select(p => new ProductSearchDocument
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Category = p.Category,
                Price = p.Price
            });

            await _searchService.BulkIndexAsync(documents, cancellationToken);

            totalIndexed += products.Count;
            lastId = products[^1].Id;

            _logger.LogInformation("Reindexed batch of {Count} products (total: {Total})",
                products.Count, totalIndexed);
        }

        _logger.LogInformation("Reindex complete. Total products indexed: {Total}", totalIndexed);
        return totalIndexed;
    }
}
