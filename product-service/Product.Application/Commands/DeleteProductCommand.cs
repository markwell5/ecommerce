using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Events.Product;
using MassTransit;
using MediatR;
using Product.Application.Caching;

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
        private readonly IProductCacheInvalidator _cacheInvalidator;

        public DeleteProductCommandHandler(
            ProductDbContext dbContext,
            IPublishEndpoint publishEndpoint,
            IProductCacheInvalidator cacheInvalidator)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products.FindAsync(new object[] { request.Id }, cancellationToken);

            if (product == null)
                return false;

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _cacheInvalidator.InvalidateProductAsync(request.Id, cancellationToken);

            await _publishEndpoint.Publish(new ProductDeleted
            {
                Id = request.Id
            }, cancellationToken);

            return true;
        }
    }
}
