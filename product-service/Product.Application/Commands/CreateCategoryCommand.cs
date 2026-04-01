using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Category.Request;
using Ecommerce.Model.Category.Response;
using MediatR;

namespace Product.Application.Commands
{
    public class CreateCategoryCommand : IRequest<CategoryResponse>
    {
        public CreateCategoryCommand(CreateCategoryRequest request)
        {
            Request = request;
        }

        public CreateCategoryRequest Request { get; }
    }

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponse>
    {
        private readonly ProductDbContext _dbContext;

        public CreateCategoryCommandHandler(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CategoryResponse> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
        {
            var category = new Entities.Category
            {
                Name = command.Request.Name,
                Slug = command.Request.Slug,
                ParentId = command.Request.ParentId
            };

            _dbContext.Categories.Add(category);
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
