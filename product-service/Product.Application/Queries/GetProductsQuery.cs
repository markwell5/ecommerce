using AutoMapper;
using Ecommerce.Model.Product.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        private readonly ProductDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetProductsQueryHandler(ProductDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _dbContext.Products.AsNoTracking().ToListAsync(cancellationToken);

            return _mapper.Map<IEnumerable<ProductResponse>>(products);
        }
    }
}
