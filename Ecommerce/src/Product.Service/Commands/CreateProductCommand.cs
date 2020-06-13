using AutoMapper;
using Core.Domain;
using Core.Domain.Events;
using MediatR;
using Product.Service.Domain;
using Product.Service.Events;
using Product.Service.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Service.Commands
{
    public class CreateProductCommand : IRequest<int>
    {
        public CreateProductCommand(string name, Guid eventId)
        {
            Name = name;
            EventId = eventId;
        }

        public string Name { get; }
        public Guid EventId { get; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IProductRespository _productRespository;
        private readonly IMapper _mapper;
        private readonly IProducer _producer;

        public CreateProductCommandHandler(IProductRespository productRespository, IMapper mapper, IProducer producer)
        {
            _productRespository = productRespository;
            _mapper = mapper;
            _producer = producer;
        }

        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var dto = _mapper.Map<ProductDto>(request);
            var productId = await _productRespository.Create(dto);

            var message = new ProductCreated(productId)
            {
                EventId = request.EventId
            };

            await _producer.Publish(Topics.ProductCreated, message);

            return productId;
        }
    }
}
