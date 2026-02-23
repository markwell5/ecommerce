using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model;
using Ecommerce.Model.Product.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace Product.Application.Queries
{
    public class SearchProductsQuery : IRequest<PagedResponse<ProductResponse>>
    {
        public string Query { get; set; }
        public string Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, PagedResponse<ProductResponse>>
    {
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;

        public SearchProductsQueryHandler(ProductDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<PagedResponse<ProductResponse>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var query = _dbContext.Products.AsNoTracking().AsQueryable();

            var hasSearchQuery = !string.IsNullOrWhiteSpace(request.Query);

            if (hasSearchQuery)
            {
                var tsQuery = EF.Functions.PlainToTsQuery("english", request.Query);
                query = query.Where(p => p.SearchVector.Matches(tsQuery));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var category = request.Category;
                query = query.Where(p => p.Category == category);
            }

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.Price >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= request.MaxPrice.Value);

            if (hasSearchQuery && (request.SortBy == null || request.SortBy.Equals("relevance", StringComparison.OrdinalIgnoreCase)))
            {
                var tsQueryForRank = EF.Functions.PlainToTsQuery("english", request.Query);
                query = query.OrderByDescending(p => p.SearchVector.Rank(tsQueryForRank));
            }
            else
            {
                query = request.SortBy?.ToLower() switch
                {
                    "name" => request.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name),
                    "price" => request.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price),
                    _ => request.SortDirection?.ToLower() == "desc"
                        ? query.OrderByDescending(p => p.Id)
                        : query.OrderBy(p => p.Id)
                };
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<ProductResponse>
            {
                Items = _mapper.Map<ProductResponse[]>(products),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
        }
    }
}
