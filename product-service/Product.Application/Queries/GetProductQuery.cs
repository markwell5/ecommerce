using AutoMapper;
using Ecommerce.Model.Product.Response;
using MediatR;
using Product.Application.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace Product.Application.Queries
{
    public class GetProductQuery : IRequest<ProductResponse>
    {
        public GetProductQuery(long key)
        {
            Key = key;
        }

        public long Key { get; }
    }

    public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetProductQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<ProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
        {
            var p = await _productRepository.Get(request.Key);

            return _mapper.Map<ProductResponse>(p);
        }
    }
}
