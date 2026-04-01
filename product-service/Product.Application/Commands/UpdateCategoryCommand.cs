using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Category.Request;
using Ecommerce.Model.Category.Response;
using MediatR;

namespace Product.Application.Commands
{
    public class UpdateCategoryCommand : IRequest<CategoryResponse>
    {
        public UpdateCategoryCommand(long id, UpdateCategoryRequest request)
        {
            Id = id;
            Request = request;
        }

        public long Id { get; }
        public UpdateCategoryRequest Request { get; }
    }

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryResponse>
    {
        private readonly ProductDbContext _dbContext;

        public UpdateCategoryCommandHandler(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CategoryResponse> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            var category = await _dbContext.Categories.FindAsync(new object[] { command.Id }, cancellationToken);

            if (category == null)
                return null;

            category.Name = command.Request.Name;
            category.Slug = command.Request.Slug;
            category.ParentId = command.Request.ParentId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Slug = category.Slug,
                ParentId = category.ParentId
            };
        }
    }
}
