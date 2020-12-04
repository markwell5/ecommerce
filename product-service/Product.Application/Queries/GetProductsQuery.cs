using AutoMapper;
using Ecommerce.Model.Product.Response;
using MediatR;
using Product.Application.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Application.Queries
{
    public class GetProductsQuery : IRequest<IEnumerable<ProductResponse>>
    {
    }

    public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IEnumerable<ProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var ps = await _productRepository.Get();

            return _mapper.Map<IEnumerable<ProductResponse>>(ps);
        }
    }
}
