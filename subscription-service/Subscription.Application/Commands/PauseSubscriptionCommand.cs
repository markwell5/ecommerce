using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Subscription;
using Ecommerce.Model.Subscription.Response;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Subscription.Application.Commands
{
    public record PauseSubscriptionCommand(long Id) : IRequest<SubscriptionResponse>;

    public class PauseSubscriptionCommandHandler : IRequestHandler<PauseSubscriptionCommand, SubscriptionResponse>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public PauseSubscriptionCommandHandler(SubscriptionDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<SubscriptionResponse> Handle(PauseSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Subscription {command.Id} not found");

            if (subscription.Status != "Active")
                throw new InvalidOperationException("Only active subscriptions can be paused");

            subscription.Status = "Paused";
            subscription.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new SubscriptionPaused
            {
                SubscriptionId = subscription.Id,
                CustomerId = subscription.CustomerId
            }, cancellationToken);

            return _mapper.Map<SubscriptionResponse>(subscription);
        }
    }
}
