using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.Subscription.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Subscription.Application.Commands
{
    public class UpdateSubscriptionCommand : IRequest<SubscriptionResponse>
    {
        public long Id { get; set; }
        public int? Quantity { get; set; }
        public string? Frequency { get; set; }
        public int? IntervalDays { get; set; }
        public long? DeliveryAddressId { get; set; }
    }

    public class UpdateSubscriptionCommandHandler : IRequestHandler<UpdateSubscriptionCommand, SubscriptionResponse>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IMapper _mapper;

        public UpdateSubscriptionCommandHandler(SubscriptionDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<SubscriptionResponse> Handle(UpdateSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.Id == command.Id, cancellationToken)
                ?? throw new InvalidOperationException($"Subscription {command.Id} not found");

            if (subscription.Status == "Cancelled")
                throw new InvalidOperationException("Cannot update a cancelled subscription");

            if (command.Quantity.HasValue)
                subscription.Quantity = command.Quantity.Value;

            if (!string.IsNullOrEmpty(command.Frequency))
            {
                subscription.Frequency = command.Frequency;
                subscription.IntervalDays = command.IntervalDays ?? command.Frequency switch
                {
                    "weekly" => 7,
                    "fortnightly" => 14,
                    "monthly" => 30,
                    _ => subscription.IntervalDays
                };
            }
            else if (command.IntervalDays.HasValue)
            {
                subscription.IntervalDays = command.IntervalDays.Value;
            }

            if (command.DeliveryAddressId.HasValue)
                subscription.DeliveryAddressId = command.DeliveryAddressId.Value;

            subscription.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<SubscriptionResponse>(subscription);
        }
    }
}
