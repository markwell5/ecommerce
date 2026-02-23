using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Product;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using MassTransit;
using MediatR;

namespace Product.Application.Commands
{
    public class UpdateProductCommand : IRequest<ProductResponse>
    {
        public UpdateProductCommand(long id, UpdateProductRequest request)
        {
            Id = id;
            Request = request;
        }

        public long Id { get; }
        public UpdateProductRequest Request { get; }
    }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
    {
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdateProductCommandHandler(ProductDbContext dbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products.FindAsync(new object[] { request.Id }, cancellationToken);

            if (product == null)
                return null;

            product.Name = request.Request.Name;
            product.Description = request.Request.Description;
            product.Price = request.Request.Price;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ProductUpdated
            {
                Id = product.Id
            }, cancellationToken);

            return _mapper.Map<ProductResponse>(product);
        }
    }
}
