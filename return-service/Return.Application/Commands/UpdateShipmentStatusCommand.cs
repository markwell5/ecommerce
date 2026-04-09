using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Return.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Return.Application.Carriers;

namespace Return.Application.Commands
{
    public class UpdateShipmentStatusCommand : IRequest<ReturnShipmentResponse>
    {
        public long ReturnRequestId { get; set; }
    }

    public class UpdateShipmentStatusCommandHandler : IRequestHandler<UpdateShipmentStatusCommand, ReturnShipmentResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly ICarrierAdapter _carrierAdapter;
        private readonly IMapper _mapper;

        public UpdateShipmentStatusCommandHandler(ReturnDbContext dbContext, ICarrierAdapter carrierAdapter, IMapper mapper)
        {
            _dbContext = dbContext;
            _carrierAdapter = carrierAdapter;
            _mapper = mapper;
        }

        public async Task<ReturnShipmentResponse> Handle(UpdateShipmentStatusCommand command, CancellationToken cancellationToken)
        {
            var shipment = await _dbContext.ReturnShipments
                .FirstOrDefaultAsync(s => s.ReturnRequestId == command.ReturnRequestId, cancellationToken)
                ?? throw new InvalidOperationException("Return shipment not found");

            var status = await _carrierAdapter.GetShipmentStatusAsync(shipment.TrackingNumber, shipment.Carrier, cancellationToken);

            shipment.Status = status.Status;
            shipment.UpdatedAt = DateTime.UtcNow;

            if (status.Status == "Delivered" && shipment.DeliveredAt == null)
            {
                shipment.DeliveredAt = DateTime.UtcNow;

                var ret = await _dbContext.ReturnRequests
                    .FirstOrDefaultAsync(r => r.Id == shipment.ReturnRequestId, cancellationToken);
                if (ret != null && ret.Status == "Approved")
                {
                    ret.Status = "Received";
                    ret.ReceivedAt = DateTime.UtcNow;
                    ret.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return _mapper.Map<ReturnShipmentResponse>(shipment);
        }
    }
}
