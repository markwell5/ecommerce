using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Return;
using Ecommerce.Model.Return.Response;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Return.Application.Commands
{
    public record ApproveReturnCommand(long ReturnRequestId, string AdminNotes) : IRequest<ReturnResponse>;

    public class ApproveReturnCommandHandler : IRequestHandler<ApproveReturnCommand, ReturnResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public ApproveReturnCommandHandler(ReturnDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> Handle(ApproveReturnCommand command, CancellationToken cancellationToken)
        {
            var ret = await _dbContext.ReturnRequests
                .FirstOrDefaultAsync(r => r.Id == command.ReturnRequestId, cancellationToken);

            if (ret == null) return null;
            if (ret.Status != "Requested")
                throw new InvalidOperationException($"Cannot approve return in status '{ret.Status}'");

            ret.Status = "Approved";
            ret.AdminNotes = command.AdminNotes;
            ret.ApprovedAt = DateTime.UtcNow;
            ret.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new ReturnApproved
            {
                ReturnRequestId = ret.Id,
                OrderId = ret.OrderId,
                ProductId = ret.ProductId,
                Quantity = ret.Quantity
            }, cancellationToken);

            return _mapper.Map<ReturnResponse>(ret);
        }
    }
}
