using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Product.Response;
using MediatR;
using Product.Application.Caching;

namespace Product.Application.Queries
{
    public class GetProductQuery : IRequest<ProductResponse>, ICacheableQuery
    {
        public GetProductQuery(long id)
        {
            Id = id;
        }

        public long Id { get; }
        public string CacheKey => $"product:item:{Id}";
    }

    public class GetProductQueryHandler : IRequestHandler<GetProductQuery, ProductResponse>
    {
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetProductQueryHandler(ProductDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products.FindAsync(new object[] { request.Id }, cancellationToken);

            return _mapper.Map<ProductResponse>(product);
        }
    }
}
