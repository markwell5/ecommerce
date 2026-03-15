using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Product.Application.Queries;

namespace Product.Application.Search;

public interface IProductSearchService
{
    Task CreateIndexIfNotExistsAsync(CancellationToken cancellationToken = default);
    Task IndexProductAsync(ProductSearchDocument document, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(long productId, CancellationToken cancellationToken = default);
    Task BulkIndexAsync(IEnumerable<ProductSearchDocument> documents, CancellationToken cancellationToken = default);
    Task<ProductSearchResult> SearchAsync(SearchProductsQuery query, CancellationToken cancellationToken = default);
}

public class ProductSearchResult
{
    public IReadOnlyList<ProductSearchDocument> Items { get; set; } = [];
    public long TotalCount { get; set; }
    public SearchFacets Facets { get; set; } = new();
}
