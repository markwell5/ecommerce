using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Subscription.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Subscription.Application.Commands
{
    public record ResumeSubscriptionCommand(long Id) : IRequest<SubscriptionResponse>;

    public class ResumeSubscriptionCommandHandler : IRequestHandler<ResumeSubscriptionCommand, SubscriptionResponse>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IMapper _mapper;

        public ResumeSubscriptionCommandHandler(SubscriptionDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<SubscriptionResponse> Handle(ResumeSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Subscription {command.Id} not found");

            if (subscription.Status != "Paused")
                throw new InvalidOperationException("Only paused subscriptions can be resumed");

            subscription.Status = "Active";
            subscription.NextRenewalAt = DateTime.UtcNow.AddDays(subscription.IntervalDays);
            subscription.FailureCount = 0;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<SubscriptionResponse>(subscription);
        }
    }
}
