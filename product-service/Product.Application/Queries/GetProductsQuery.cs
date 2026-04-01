using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model;
using Ecommerce.Model.Product.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Product.Application.Caching;

namespace Product.Application.Queries
{
    public class GetProductsQuery : IRequest<PagedResponse<ProductResponse>>, ICacheableQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
        public string Search { get; set; }
        public string Category { get; set; }

        public string CacheKey =>
            $"product:query:list:{Page}:{PageSize}:{SortBy}:{SortDirection}:{Search}:{Category}";
    }

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResponse<ProductResponse>>
    {
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(ProductDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<PagedResponse<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 100);

            var query = _dbContext.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = request.Search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(search)
                    || p.Description.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(request.Category))
            {
                var categorySlug = request.Category;
                query = query.Where(p => p.ProductCategories.Any(pc => pc.Category.Slug == categorySlug));
            }

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
