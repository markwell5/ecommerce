using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Return.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Return.Application.Queries
{
    public record GetReturnShipmentQuery(long ReturnRequestId) : IRequest<ReturnShipmentResponse>;

    public class GetReturnShipmentQueryHandler : IRequestHandler<GetReturnShipmentQuery, ReturnShipmentResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IMapper _mapper;

        public GetReturnShipmentQueryHandler(ReturnDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ReturnShipmentResponse> Handle(GetReturnShipmentQuery request, CancellationToken cancellationToken)
        {
            var shipment = await _dbContext.ReturnShipments
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ReturnRequestId == request.ReturnRequestId, cancellationToken);

            return shipment == null ? null : _mapper.Map<ReturnShipmentResponse>(shipment);
        }
    }
}
