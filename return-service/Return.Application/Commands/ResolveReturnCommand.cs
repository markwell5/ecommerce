using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Return;
using Ecommerce.Model.Return.Response;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Return.Application.Policies;

namespace Return.Application.Commands
{
    public class ResolveReturnCommand : IRequest<ReturnResponse>
    {
        public long ReturnRequestId { get; set; }
        public string Resolution { get; set; } = string.Empty; // full_refund, partial_refund, exchange
        public decimal RefundAmount { get; set; }
    }

    public class ResolveReturnCommandHandler : IRequestHandler<ResolveReturnCommand, ReturnResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public ResolveReturnCommandHandler(ReturnDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> Handle(ResolveReturnCommand command, CancellationToken cancellationToken)
        {
            var ret = await _dbContext.ReturnRequests
                .FirstOrDefaultAsync(r => r.Id == command.ReturnRequestId, cancellationToken);

            if (ret == null) return null;
            if (ret.Status != "Approved" && ret.Status != "Received")
                throw new InvalidOperationException($"Cannot resolve return in status '{ret.Status}'");

            var restockingFee = ReturnPolicy.CalculateRestockingFee(ret.Reason, ret.CreatedAt, command.RefundAmount);
            var finalRefund = command.RefundAmount - restockingFee;

            ret.Resolution = command.Resolution;
            ret.RefundAmount = finalRefund;
            ret.RestockingFee = restockingFee;
            ret.Status = command.Resolution == "exchange" ? "Exchanged" : "Refunded";
            ret.ResolvedAt = DateTime.UtcNow;
            ret.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (command.Resolution is "full_refund" or "partial_refund")
            {
                await _publishEndpoint.Publish(new RefundRequested
                {
                    ReturnRequestId = ret.Id,
                    OrderId = ret.OrderId,
                    CustomerId = ret.CustomerId,
                    Amount = finalRefund
                }, cancellationToken);
            }

            return _mapper.Map<ReturnResponse>(ret);
        }
    }
}
