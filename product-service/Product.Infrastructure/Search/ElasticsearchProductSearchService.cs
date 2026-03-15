using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Product.Application.Queries;
using Product.Application.Search;

namespace Product.Infrastructure.Search;

public class ElasticsearchProductSearchService : IProductSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;
    private readonly ILogger<ElasticsearchProductSearchService> _logger;

    public ElasticsearchProductSearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchSettings> settings,
        ILogger<ElasticsearchProductSearchService> logger)
    {
        _client = client;
        _indexName = settings.Value.IndexName;
        _logger = logger;
    }

    public async Task CreateIndexIfNotExistsAsync(CancellationToken cancellationToken = default)
    {
        var existsResponse = await _client.Indices.ExistsAsync(_indexName, cancellationToken);
        if (existsResponse.Exists)
            return;

        var createResponse = await _client.Indices.CreateAsync(_indexName, c => c
            .Mappings(m => m
                .Properties(new Properties
                {
                    { "id", new LongNumberProperty() },
                    { "name", new TextProperty { Boost = 3.0, Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "description", new TextProperty { Boost = 1.0 } },
                    { "category", new KeywordProperty() },
                    { "price", new DoubleNumberProperty() }
                })
            ), cancellationToken);

        if (!createResponse.IsValidResponse)
        {
            _logger.LogError("Failed to create Elasticsearch index {IndexName}: {Error}",
                _indexName, createResponse.DebugInformation);
        }
    }

    public async Task IndexProductAsync(ProductSearchDocument document, CancellationToken cancellationToken = default)
    {
        var response = await _client.IndexAsync(document, _indexName, document.Id.ToString(), cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Failed to index product {ProductId}: {Error}",
                document.Id, response.DebugInformation);
        }
    }

    public async Task DeleteProductAsync(long productId, CancellationToken cancellationToken = default)
    {
        var response = await _client.DeleteAsync(_indexName, productId.ToString(), cancellationToken);

        if (!response.IsValidResponse && response.Result != Result.NotFound)
        {
            _logger.LogError("Failed to delete product {ProductId} from index: {Error}",
                productId, response.DebugInformation);
        }
    }

    public async Task BulkIndexAsync(IEnumerable<ProductSearchDocument> documents, CancellationToken cancellationToken = default)
    {
        var response = await _client.BulkAsync(b => b
            .Index(_indexName)
            .IndexMany(documents, (op, doc) => op.Id(doc.Id.ToString())),
            cancellationToken);

        if (response.Errors)
        {
            var errorCount = response.ItemsWithErrors.Count();
            _logger.LogError("Bulk index completed with {ErrorCount} errors", errorCount);
        }
    }

    public async Task<ProductSearchResult> SearchAsync(SearchProductsQuery query, CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var from = (page - 1) * pageSize;

        var response = await _client.SearchAsync<ProductSearchDocument>(s =>
        {
            s.Index(_indexName)
                .From(from)
                .Size(pageSize)
                .Aggregations(aggs => aggs
                    .Add("categories", agg => agg.Terms(t => t.Field("category").Size(50)))
                    .Add("price_stats", agg => agg.Stats(st => st.Field("price")))
                );

            BuildQuery(s, query);
            BuildSort(s, query);
        }, cancellationToken);

        if (!response.IsValidResponse)
        {
            _logger.LogError("Elasticsearch search failed: {Error}", response.DebugInformation);
            return new ProductSearchResult();
        }

        var facets = ExtractFacets(response);

        return new ProductSearchResult
        {
            Items = response.Documents.ToList(),
            TotalCount = response.Total,
            Facets = facets
        };
    }

    private static void BuildQuery(SearchRequestDescriptor<ProductSearchDocument> s, SearchProductsQuery query)
    {
        var hasSearchQuery = !string.IsNullOrWhiteSpace(query.Query);
        var hasCategory = !string.IsNullOrWhiteSpace(query.Category);
        var hasPriceFilter = query.MinPrice.HasValue || query.MaxPrice.HasValue;

        if (!hasSearchQuery && !hasCategory && !hasPriceFilter)
        {
            s.Query(q => q.MatchAll(new MatchAllQuery()));
            return;
        }

        s.Query(q => q.Bool(b =>
        {
            if (hasSearchQuery)
            {
                b.Must(m => m.MultiMatch(mm => mm
                    .Query(query.Query)
                    .Fields(new[] { "name^3", "description" })
                    .Fuzziness(new Fuzziness("AUTO"))
                    .Type(TextQueryType.BestFields)
                ));
            }

            var filters = new List<Action<QueryDescriptor<ProductSearchDocument>>>();

            if (hasCategory)
            {
                filters.Add(f => f.Term(t => t.Field("category").Value(query.Category)));
            }

            if (hasPriceFilter)
            {
                filters.Add(f => f.Range(r => r.NumberRange(nr =>
                {
                    nr.Field("price");
                    if (query.MinPrice.HasValue)
                        nr.Gte((double)query.MinPrice.Value);
                    if (query.MaxPrice.HasValue)
                        nr.Lte((double)query.MaxPrice.Value);
                })));
            }

            if (filters.Count > 0)
            {
                b.Filter(filters.ToArray());
            }
        }));
    }

    private static void BuildSort(SearchRequestDescriptor<ProductSearchDocument> s, SearchProductsQuery query)
    {
        var hasSearchQuery = !string.IsNullOrWhiteSpace(query.Query);
        if (hasSearchQuery && (query.SortBy == null || query.SortBy.Equals("relevance", StringComparison.OrdinalIgnoreCase)))
            return;

        var sortField = query.SortBy?.ToLower() switch
        {
            "name" => "name.keyword",
            "price" => "price",
            _ => "id"
        };

        var sortOrder = query.SortDirection?.ToLower() == "desc"
            ? SortOrder.Desc : SortOrder.Asc;

        s.Sort(sort => sort.Field(sortField, new FieldSort { Order = sortOrder }));
    }

    private static SearchFacets ExtractFacets(SearchResponse<ProductSearchDocument> response)
    {
        var facets = new SearchFacets();

        if (response.Aggregations == null)
            return facets;

        if (response.Aggregations.TryGetValue("categories", out var catAgg))
        {
            var termsAgg = catAgg as StringTermsAggregate;
            if (termsAgg != null)
            {
                foreach (var bucket in termsAgg.Buckets)
                {
                    var termBucket = bucket as StringTermsBucket;
                    if (termBucket != null)
                        facets.Categories[termBucket.Key.ToString()] = termBucket.DocCount;
                }
            }
        }

        if (response.Aggregations.TryGetValue("price_stats", out var priceAgg))
        {
            var statsAgg = priceAgg as StatsAggregate;
            if (statsAgg != null)
            {
                facets.MinPrice = statsAgg.Min.HasValue ? (decimal)statsAgg.Min.Value : null;
                facets.MaxPrice = statsAgg.Max.HasValue ? (decimal)statsAgg.Max.Value : null;
            }
        }

        return facets;
    }
}
