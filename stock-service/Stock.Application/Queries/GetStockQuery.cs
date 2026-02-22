using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Stock.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Stock.Application.Queries
{
    public class GetStockQuery : IRequest<StockResponse>
    {
        public GetStockQuery(long productId)
        {
            ProductId = productId;
        }

        public long ProductId { get; }
    }

    public class GetStockQueryHandler : IRequestHandler<GetStockQuery, StockResponse>
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetStockQueryHandler(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<StockResponse> Handle(GetStockQuery request, CancellationToken cancellationToken)
        {
            var stockItem = await _dbContext.StockItems
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);

            if (stockItem == null)
                return null;

            return _mapper.Map<StockResponse>(stockItem);
        }
    }
}
