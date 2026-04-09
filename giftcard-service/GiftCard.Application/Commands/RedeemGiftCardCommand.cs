using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.GiftCard;
using Ecommerce.Model.GiftCard.Response;
using GiftCard.Application.Entities;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftCard.Application.Commands
{
    public class RedeemGiftCardCommand : IRequest<GiftCardTransactionResponse>
    {
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? OrderId { get; set; }
    }

    public class RedeemGiftCardCommandHandler : IRequestHandler<RedeemGiftCardCommand, GiftCardTransactionResponse>
    {
        private readonly GiftCardDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public RedeemGiftCardCommandHandler(GiftCardDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<GiftCardTransactionResponse> Handle(RedeemGiftCardCommand command, CancellationToken cancellationToken)
        {
            if (command.Amount <= 0)
                throw new InvalidOperationException("Redemption amount must be greater than zero");

            var giftCard = await _dbContext.GiftCards
                .FirstOrDefaultAsync(g => g.Code == command.Code, cancellationToken)
                ?? throw new InvalidOperationException("Gift card not found");

            if (giftCard.Status != "Active")
                throw new InvalidOperationException($"Gift card is {giftCard.Status}");

            if (giftCard.ExpiresAt.HasValue && giftCard.ExpiresAt.Value < DateTime.UtcNow)
            {
                giftCard.Status = "Expired";
                giftCard.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("Gift card has expired");
            }

            if (giftCard.CurrentBalance < command.Amount)
                throw new InvalidOperationException("Insufficient gift card balance");

            giftCard.CurrentBalance -= command.Amount;
            giftCard.UpdatedAt = DateTime.UtcNow;

            var transaction = new GiftCardTransaction
            {
                GiftCardId = giftCard.Id,
                Type = "redeem",
                Amount = -command.Amount,
                BalanceAfter = giftCard.CurrentBalance,
                OrderId = command.OrderId,
                Description = $"Redeemed {command.Amount:C} from gift card"
            };

            _dbContext.GiftCardTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new GiftCardRedeemed
            {
                Code = command.Code,
                Amount = command.Amount,
                OrderId = command.OrderId,
                RemainingBalance = giftCard.CurrentBalance
            }, cancellationToken);

            return _mapper.Map<GiftCardTransactionResponse>(transaction);
        }
    }
}
