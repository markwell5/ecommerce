using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace Product.Application.Commands
{
    public class DeleteCategoryCommand : IRequest<bool>
    {
        public DeleteCategoryCommand(long id)
        {
            Id = id;
        }

        public long Id { get; }
    }

    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, bool>
    {
        private readonly ProductDbContext _dbContext;

        public DeleteCategoryCommandHandler(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
        {
            var category = await _dbContext.Categories.FindAsync(new object[] { command.Id }, cancellationToken);

            if (category == null)
                return false;

            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
