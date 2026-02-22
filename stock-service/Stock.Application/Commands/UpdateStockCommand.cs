using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Stock.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Stock.Application.Commands
{
    public class UpdateStockCommand : IRequest<StockResponse>
    {
        public UpdateStockCommand(long productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public long ProductId { get; }
        public int Quantity { get; }
    }

    public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, StockResponse>
    {
        private readonly StockDbContext _dbContext;
        private readonly IMapper _mapper;

        public UpdateStockCommandHandler(StockDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<StockResponse> Handle(UpdateStockCommand command, CancellationToken cancellationToken)
        {
            var stockItem = await _dbContext.StockItems
                .FirstOrDefaultAsync(s => s.ProductId == command.ProductId, cancellationToken);

            if (stockItem == null)
                return null;

            stockItem.AvailableQuantity = command.Quantity;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<StockResponse>(stockItem);
        }
    }
}
