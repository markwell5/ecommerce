using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Subscription;
using Ecommerce.Model.Subscription.Response;
using MassTransit;
using MediatR;

namespace Subscription.Application.Commands
{
    public class CreateSubscriptionCommand : IRequest<SubscriptionResponse>
    {
        public string CustomerId { get; set; } = string.Empty;
        public long ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Frequency { get; set; } = "monthly";
        public int IntervalDays { get; set; }
        public decimal DiscountPercent { get; set; }
        public long DeliveryAddressId { get; set; }
    }

    public class CreateSubscriptionCommandHandler : IRequestHandler<CreateSubscriptionCommand, SubscriptionResponse>
    {
        private readonly SubscriptionDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public CreateSubscriptionCommandHandler(SubscriptionDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<SubscriptionResponse> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken)
        {
            var intervalDays = command.IntervalDays > 0
                ? command.IntervalDays
                : command.Frequency switch
                {
                    "weekly" => 7,
                    "fortnightly" => 14,
                    "monthly" => 30,
                    _ => 30
                };

            var subscription = new Entities.Subscription
            {
                CustomerId = command.CustomerId,
                ProductId = command.ProductId,
                ProductName = command.ProductName,
                Quantity = command.Quantity,
                Frequency = command.Frequency,
                IntervalDays = intervalDays,
                DiscountPercent = command.DiscountPercent,
                DeliveryAddressId = command.DeliveryAddressId,
                Status = "Active",
                NextRenewalAt = DateTime.UtcNow.AddDays(intervalDays)
            };

            _dbContext.Subscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new SubscriptionCreated
            {
                SubscriptionId = subscription.Id,
                CustomerId = subscription.CustomerId,
                ProductId = subscription.ProductId,
                Frequency = subscription.Frequency
            }, cancellationToken);

            return _mapper.Map<SubscriptionResponse>(subscription);
        }
    }
}
