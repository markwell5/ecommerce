using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Subscription.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Subscription.Application.Entities;

namespace Subscription.Application.Commands
{
    public record SkipNextDeliveryCommand(long Id) : IRequest<SubscriptionResponse>;

    public class SkipNextDeliveryCommandHandler : IRequestHandler<SkipNextDeliveryCommand, SubscriptionResponse>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IMapper _mapper;

        public SkipNextDeliveryCommandHandler(SubscriptionDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<SubscriptionResponse> Handle(SkipNextDeliveryCommand command, CancellationToken cancellationToken)
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Subscription {command.Id} not found");

            if (subscription.Status != "Active")
                throw new InvalidOperationException("Can only skip delivery for active subscriptions");

            // Record the skip
            _dbContext.RenewalHistories.Add(new RenewalHistory
            {
                SubscriptionId = subscription.Id,
                Status = "Skipped",
                CreatedAt = DateTime.UtcNow
            });

            subscription.NextRenewalAt = subscription.NextRenewalAt.AddDays(subscription.IntervalDays);
            subscription.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<SubscriptionResponse>(subscription);
        }
    }
}
