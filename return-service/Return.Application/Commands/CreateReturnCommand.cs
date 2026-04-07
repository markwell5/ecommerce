using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Return;
using Ecommerce.Model.Return.Response;
using MassTransit;
using MediatR;
using Return.Application.Entities;
using Return.Application.Policies;

namespace Return.Application.Commands
{
    public class CreateReturnCommand : IRequest<ReturnResponse>
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime DeliveredAt { get; set; }
    }

    public class CreateReturnCommandHandler : IRequestHandler<CreateReturnCommand, ReturnResponse>
    {
        private readonly ReturnDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public CreateReturnCommandHandler(ReturnDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> Handle(CreateReturnCommand command, CancellationToken cancellationToken)
        {
            if (!ReturnPolicy.IsWithinReturnWindow(command.DeliveredAt))
                throw new InvalidOperationException("Return window has expired");

            var rmaNumber = $"RMA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            var autoApprove = ReturnPolicy.ShouldAutoApprove(command.Reason, command.DeliveredAt);

            var returnRequest = new ReturnRequest
            {
                RmaNumber = rmaNumber,
                OrderId = command.OrderId,
                CustomerId = command.CustomerId,
                ProductId = command.ProductId,
                Quantity = command.Quantity,
                Reason = command.Reason,
                Status = autoApprove ? "Approved" : "Requested",
                AutoApproved = autoApprove,
                ApprovedAt = autoApprove ? DateTime.UtcNow : null
            };

            _dbContext.ReturnRequests.Add(returnRequest);
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (autoApprove)
            {
                await _publishEndpoint.Publish(new ReturnApproved
                {
                    ReturnRequestId = returnRequest.Id,
                    OrderId = returnRequest.OrderId,
                    ProductId = returnRequest.ProductId,
                    Quantity = returnRequest.Quantity
                }, cancellationToken);
            }

            return _mapper.Map<ReturnResponse>(returnRequest);
        }
    }
}
