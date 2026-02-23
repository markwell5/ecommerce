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
    public class CreateProductCommand : IRequest<ProductResponse>
    {
        public CreateProductCommand(CreateProductRequest request)
        {
            Request = request;
        }

        public CreateProductRequest Request { get; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateProductCommandHandler(ProductDbContext dbContext, IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Entities.Product>(request.Request);

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ProductCreated
            {
                Id = product.Id
            }, cancellationToken);

            return _mapper.Map<ProductResponse>(product);
        }
    }
}
