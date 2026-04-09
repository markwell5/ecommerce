using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Return.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Return.Application.Carriers;
using Return.Application.Entities;

namespace Return.Application.Commands
{
    public class GenerateReturnLabelCommand : IRequest<ReturnShipmentResponse>
    {
        public long ReturnRequestId { get; set; }
        public string Carrier { get; set; } = "royal_mail";
    }

    public class GenerateReturnLabelCommandHandler : IRequestHandler<GenerateReturnLabelCommand, ReturnShipmentResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly ICarrierAdapter _carrierAdapter;
        private readonly IMapper _mapper;

        public GenerateReturnLabelCommandHandler(ReturnDbContext dbContext, ICarrierAdapter carrierAdapter, IMapper mapper)
        {
            _dbContext = dbContext;
            _carrierAdapter = carrierAdapter;
            _mapper = mapper;
        }

        public async Task<ReturnShipmentResponse> Handle(GenerateReturnLabelCommand command, CancellationToken cancellationToken)
        {
            var ret = await _dbContext.ReturnRequests
                .FirstOrDefaultAsync(r => r.Id == command.ReturnRequestId, cancellationToken)
                ?? throw new InvalidOperationException("Return request not found");

            if (ret.Status != "Approved")
                throw new InvalidOperationException($"Cannot generate label for return in status '{ret.Status}'");

            var existing = await _dbContext.ReturnShipments
                .FirstOrDefaultAsync(s => s.ReturnRequestId == command.ReturnRequestId, cancellationToken);

            if (existing != null)
                return _mapper.Map<ReturnShipmentResponse>(existing);

            var label = await _carrierAdapter.GenerateReturnLabelAsync(ret.RmaNumber, command.Carrier, cancellationToken);

            var shipment = new ReturnShipment
            {
                ReturnRequestId = ret.Id,
                Carrier = label.Carrier,
                TrackingNumber = label.TrackingNumber,
                LabelUrl = label.LabelUrl,
                Status = "LabelGenerated"
            };

            _dbContext.ReturnShipments.Add(shipment);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<ReturnShipmentResponse>(shipment);
        }
    }
}
