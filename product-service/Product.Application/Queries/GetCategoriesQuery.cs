using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Category.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Product.Application.Queries
{
    public class GetCategoriesQuery : IRequest<List<CategoryResponse>>
    {
    }

    public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryResponse>>
    {
        private readonly ProductDbContext _dbContext;

        public GetCategoriesQueryHandler(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Include(c => c.Children)
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            return categories.Select(MapCategory).ToList();
        }

        private static CategoryResponse MapCategory(Entities.Category entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            ParentId = entity.ParentId,
            Children = entity.Children?.Select(MapCategory).ToList() ?? new()
        };
    }
}
