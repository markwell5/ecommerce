using AutoMapper;
using Ecommerce.Model.Product.Response;
using MediatR;
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
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetProductQueryHandler(ProductDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
        {
            var product = await _dbContext.Products.FindAsync(new object[] { request.Key }, cancellationToken);

            return _mapper.Map<ProductResponse>(product);
        }
    }
}
