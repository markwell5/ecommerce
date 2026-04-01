using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Product.Application.Commands
{
    public class RemoveProductCategoryCommand : IRequest<bool>
    {
        public RemoveProductCategoryCommand(long productId, long categoryId)
        {
            ProductId = productId;
            CategoryId = categoryId;
        }

        public long ProductId { get; }
        public long CategoryId { get; }
    }

    public class RemoveProductCategoryCommandHandler : IRequestHandler<RemoveProductCategoryCommand, bool>
    {
        private readonly ProductDbContext _dbContext;

        public RemoveProductCategoryCommandHandler(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(RemoveProductCategoryCommand command, CancellationToken cancellationToken)
        {
            var link = await _dbContext.ProductCategories
                .FirstOrDefaultAsync(pc => pc.ProductId == command.ProductId && pc.CategoryId == command.CategoryId, cancellationToken);

            if (link == null)
                return false;

            _dbContext.ProductCategories.Remove(link);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
