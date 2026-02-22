using AutoMapper;
using Ecommerce.Events.Product;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using Ecommerce.Shared.Infrastructure;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IEventNotifier _eventNotifier;

        public CreateProductCommandHandler(ProductDbContext dbContext, IMapper mapper, IEventNotifier eventNotifier)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _eventNotifier = eventNotifier;
        }

        public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = _mapper.Map<Entities.Product>(request.Request);

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _eventNotifier.Notify(new ProductCreated
            {
                Key = product.Id
            });

            return _mapper.Map<ProductResponse>(product);
        }
    }
}
