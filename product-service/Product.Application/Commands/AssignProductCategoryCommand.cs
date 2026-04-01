using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Product.Application.Commands
{
    public class AssignProductCategoryCommand : IRequest<bool>
    {
        public AssignProductCategoryCommand(long productId, long categoryId)
        {
            ProductId = productId;
            CategoryId = categoryId;
        }

        public long ProductId { get; }
        public long CategoryId { get; }
    }

    public class AssignProductCategoryCommandHandler : IRequestHandler<AssignProductCategoryCommand, bool>
    {
        private readonly ProductDbContext _dbContext;

        public AssignProductCategoryCommandHandler(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(AssignProductCategoryCommand command, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.ProductCategories
                .AnyAsync(pc => pc.ProductId == command.ProductId && pc.CategoryId == command.CategoryId, cancellationToken);

            if (exists)
                return true;

            _dbContext.ProductCategories.Add(new Entities.ProductCategory
            {
                ProductId = command.ProductId,
                CategoryId = command.CategoryId
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
