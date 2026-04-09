using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Loyalty;
using Ecommerce.Model.Loyalty.Response;
using Loyalty.Application.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Loyalty.Application.Commands
{
    public class CreditPointsCommand : IRequest<PointsTransactionResponse>
    {
        public string CustomerId { get; set; } = string.Empty;
        public int Points { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? OrderId { get; set; }
    }

    public class CreditPointsCommandHandler : IRequestHandler<CreditPointsCommand, PointsTransactionResponse>
    {
        private readonly LoyaltyDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public CreditPointsCommandHandler(LoyaltyDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<PointsTransactionResponse> Handle(CreditPointsCommand command, CancellationToken cancellationToken)
        {
            var account = await _dbContext.LoyaltyAccounts
                .FirstOrDefaultAsync(a => a.CustomerId == command.CustomerId, cancellationToken);

            if (account == null)
            {
                account = new LoyaltyAccount
                {
                    CustomerId = command.CustomerId
                };
                _dbContext.LoyaltyAccounts.Add(account);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            var adjustedPoints = (int)(command.Points * account.PointsMultiplier);
            account.PointsBalance += adjustedPoints;
            account.LifetimePoints += adjustedPoints;
            account.LastActivityAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;

            var transaction = new PointsTransaction
            {
                LoyaltyAccountId = account.Id,
                CustomerId = command.CustomerId,
                Type = "earn",
                Points = adjustedPoints,
                BalanceAfter = account.PointsBalance,
                Description = command.Description,
                OrderId = command.OrderId
            };

            _dbContext.PointsTransactions.Add(transaction);

            // Check for tier upgrade
            var previousTier = account.Tier;
            UpdateTier(account);

            await _dbContext.SaveChangesAsync(cancellationToken);

            if (previousTier != account.Tier)
            {
                await _publishEndpoint.Publish(new TierChanged
                {
                    CustomerId = account.CustomerId,
                    PreviousTier = previousTier,
                    NewTier = account.Tier
                }, cancellationToken);
            }

            return _mapper.Map<PointsTransactionResponse>(transaction);
        }

        private static void UpdateTier(LoyaltyAccount account)
        {
            var (tier, multiplier) = account.LifetimePoints switch
            {
                >= 10000 => ("Platinum", 2.5),
                >= 5000 => ("Gold", 2.0),
                >= 2000 => ("Silver", 1.5),
                _ => ("Bronze", 1.0)
            };

            account.Tier = tier;
            account.PointsMultiplier = multiplier;
        }
    }
}
