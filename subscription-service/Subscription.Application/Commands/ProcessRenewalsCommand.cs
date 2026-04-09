using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Subscription;
using Ecommerce.Model.Subscription.Response;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Subscription.Application.Entities;

namespace Subscription.Application.Commands
{
    public record ProcessRenewalsCommand : IRequest<List<SubscriptionResponse>>;

    public class ProcessRenewalsCommandHandler : IRequestHandler<ProcessRenewalsCommand, List<SubscriptionResponse>>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public ProcessRenewalsCommandHandler(SubscriptionDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<List<SubscriptionResponse>> Handle(ProcessRenewalsCommand command, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var dueSubscriptions = await _dbContext.Subscriptions
                .Where(s => s.Status == "Active" && s.NextRenewalAt <= now)
                .ToListAsync(cancellationToken);

            var processed = new List<SubscriptionResponse>();

            foreach (var subscription in dueSubscriptions)
            {
                var orderId = Guid.NewGuid().ToString();

                _dbContext.RenewalHistories.Add(new RenewalHistory
                {
                    SubscriptionId = subscription.Id,
                    OrderId = orderId,
                    Status = "Success",
                    CreatedAt = DateTime.UtcNow
                });

                subscription.LastRenewedAt = now;
                subscription.NextRenewalAt = now.AddDays(subscription.IntervalDays);
                subscription.UpdatedAt = now;

                await _publishEndpoint.Publish(new SubscriptionRenewed
                {
                    SubscriptionId = subscription.Id,
                    CustomerId = subscription.CustomerId,
                    OrderId = orderId
                }, cancellationToken);

                processed.Add(_mapper.Map<SubscriptionResponse>(subscription));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return processed;
        }
    }
}
