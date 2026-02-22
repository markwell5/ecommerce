using Ecommerce.Events.Product;
using MassTransit;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Application.Commands
{
    public class DeleteProductCommand : IRequest<bool>
    {
        public DeleteProductCommand(long id)
        {
            Id = id;
        }

        public long Id { get; }
    }

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly ProductDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public DeleteProductCommandHandler(ProductDbContext dbContext, IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products.FindAsync(new object[] { request.Id }, cancellationToken);

            if (product == null)
                return false;

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ProductDeleted
            {
                Id = request.Id
            }, cancellationToken);

            return true;
        }
    }
}
