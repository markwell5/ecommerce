using AutoMapper;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using MediatR;
using Product.Application.Domain;
using Product.Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;

        public CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper)
        {
            this.productRepository = productRepository;
            this.mapper = mapper;
        }

        public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var dto = mapper.Map<ProductDto>(request.Request);
            var created = await productRepository.Create(dto);

            return mapper.Map<ProductResponse>(created);
        }
    }
}
