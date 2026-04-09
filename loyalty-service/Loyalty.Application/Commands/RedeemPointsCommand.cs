using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.Loyalty;
using Ecommerce.Model.Loyalty.Response;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Loyalty.Application.Entities;

namespace Loyalty.Application.Commands
{
    public class RedeemPointsCommand : IRequest<PointsTransactionResponse>
    {
        public string CustomerId { get; set; } = string.Empty;
        public int Points { get; set; }
        public string? OrderId { get; set; }
    }

    public class RedeemPointsCommandHandler : IRequestHandler<RedeemPointsCommand, PointsTransactionResponse>
    {
        private readonly LoyaltyDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        private const int MinRedemptionPoints = 100;
        private const decimal PointsToCurrencyRate = 0.01m; // 100 points = 1.00

        public RedeemPointsCommandHandler(LoyaltyDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<PointsTransactionResponse> Handle(RedeemPointsCommand command, CancellationToken cancellationToken)
        {
            if (command.Points < MinRedemptionPoints)
                throw new InvalidOperationException($"Minimum redemption is {MinRedemptionPoints} points");

            var account = await _dbContext.LoyaltyAccounts
                .FirstOrDefaultAsync(a => a.CustomerId == command.CustomerId, cancellationToken)
                ?? throw new InvalidOperationException("Loyalty account not found");

            if (account.PointsBalance < command.Points)
                throw new InvalidOperationException("Insufficient points balance");

            account.PointsBalance -= command.Points;
            account.LastActivityAt = DateTime.UtcNow;
            account.UpdatedAt = DateTime.UtcNow;

            var transaction = new PointsTransaction
            {
                LoyaltyAccountId = account.Id,
                CustomerId = command.CustomerId,
                Type = "redeem",
                Points = -command.Points,
                BalanceAfter = account.PointsBalance,
                Description = $"Redeemed {command.Points} points for order discount",
                OrderId = command.OrderId
            };

            _dbContext.PointsTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new PointsRedeemed
            {
                CustomerId = command.CustomerId,
                Points = command.Points,
                DiscountAmount = command.Points * PointsToCurrencyRate,
                OrderId = command.OrderId
            }, cancellationToken);

            return _mapper.Map<PointsTransactionResponse>(transaction);
        }
    }
}
