using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model;
using Ecommerce.Model.Category.Response;
using Ecommerce.Model.Product.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Product.Application.Queries
{
    public class GetCategoryBySlugQuery : IRequest<CategoryWithProductsResponse>
    {
        public GetCategoryBySlugQuery(string slug, int page, int pageSize)
        {
            Slug = slug;
            Page = page;
            PageSize = pageSize;
        }

        public string Slug { get; }
        public int Page { get; }
        public int PageSize { get; }
    }

    public class CategoryWithProductsResponse
    {
        public CategoryResponse Category { get; set; }
        public PagedResponse<ProductResponse> Products { get; set; }
    }

    public class GetCategoryBySlugQueryHandler : IRequestHandler<GetCategoryBySlugQuery, CategoryWithProductsResponse>
    {
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetCategoryBySlugQueryHandler(ProductDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<CategoryWithProductsResponse> Handle(GetCategoryBySlugQuery request, CancellationToken cancellationToken)
        {
            var category = await _dbContext.Categories
                .AsNoTracking()
                .Include(c => c.Children)
                .FirstOrDefaultAsync(c => c.Slug == request.Slug, cancellationToken);

            if (category == null)
                return null;

            var page = System.Math.Max(1, request.Page);
            var pageSize = System.Math.Clamp(request.PageSize, 1, 100);

            // Get all category IDs (this category + children) for inclusive results
            var categoryIds = new List<long> { category.Id };
            categoryIds.AddRange(category.Children.Select(c => c.Id));

            var query = _dbContext.ProductCategories
                .AsNoTracking()
                .Where(pc => categoryIds.Contains(pc.CategoryId))
                .Select(pc => pc.Product)
                .Distinct();

            var totalCount = await query.CountAsync(cancellationToken);
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new CategoryWithProductsResponse
            {
                Category = new CategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name,
                    Slug = category.Slug,
                    ParentId = category.ParentId,
                    Children = category.Children.Select(c => new CategoryResponse
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Slug = c.Slug,
                        ParentId = c.ParentId
                    }).ToList()
                },
                Products = new PagedResponse<ProductResponse>
                {
                    Items = _mapper.Map<ProductResponse[]>(products),
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)System.Math.Ceiling((double)totalCount / pageSize)
                }
            };
        }
    }
}
