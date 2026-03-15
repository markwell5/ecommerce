using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Product.Response;
using MediatR;
using Product.Application.Caching;
using Product.Application.Search;

namespace Product.Application.Queries
{
    public class SearchProductsQuery : IRequest<ProductSearchResponse>, ICacheableQuery
    {
        public string Query { get; set; }
        public string Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public string CacheKey =>
            $"product:query:search:{Query}:{Category}:{MinPrice}:{MaxPrice}:{SortBy}:{SortDirection}:{Page}:{PageSize}";
    }

    public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, ProductSearchResponse>
    {
        private readonly IProductSearchService _searchService;

        public SearchProductsQueryHandler(IProductSearchService searchService)
        {
            _searchService = searchService;
        }

        public async Task<ProductSearchResponse> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var result = await _searchService.SearchAsync(request, cancellationToken);

            return new ProductSearchResponse
            {
                Items = result.Items.Select(d => new ProductResponse
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    Category = d.Category,
                    Price = d.Price
                }).ToArray(),
                Page = page,
                PageSize = pageSize,
                TotalCount = (int)result.TotalCount,
                TotalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize),
                CategoryFacets = result.Facets.Categories,
                PriceRangeMin = result.Facets.MinPrice,
                PriceRangeMax = result.Facets.MaxPrice
            };
        }
    }
}
