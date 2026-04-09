using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Model.GiftCard.Response;
using GiftCard.Application.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GiftCard.Application.Commands
{
    public class TopUpGiftCardCommand : IRequest<GiftCardTransactionResponse>
    {
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class TopUpGiftCardCommandHandler : IRequestHandler<TopUpGiftCardCommand, GiftCardTransactionResponse>
    {
        private readonly GiftCardDbContext _dbContext;
        private readonly IMapper _mapper;

        public TopUpGiftCardCommandHandler(GiftCardDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GiftCardTransactionResponse> Handle(TopUpGiftCardCommand command, CancellationToken cancellationToken)
        {
            if (command.Amount <= 0)
                throw new InvalidOperationException("Top-up amount must be greater than zero");

            var giftCard = await _dbContext.GiftCards
                .FirstOrDefaultAsync(g => g.Code == command.Code, cancellationToken)
                ?? throw new InvalidOperationException("Gift card not found");

            if (giftCard.Status != "Active")
                throw new InvalidOperationException($"Gift card is {giftCard.Status}");

            giftCard.CurrentBalance += command.Amount;
            giftCard.UpdatedAt = DateTime.UtcNow;

            var transaction = new GiftCardTransaction
            {
                GiftCardId = giftCard.Id,
                Type = "topup",
                Amount = command.Amount,
                BalanceAfter = giftCard.CurrentBalance,
                Description = $"Top-up of {command.Amount:C}"
            };

            _dbContext.GiftCardTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<GiftCardTransactionResponse>(transaction);
        }
    }
}
