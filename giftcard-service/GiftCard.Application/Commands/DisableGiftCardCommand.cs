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
    public class DisableGiftCardCommand : IRequest<GiftCardTransactionResponse>
    {
        public string Code { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class DisableGiftCardCommandHandler : IRequestHandler<DisableGiftCardCommand, GiftCardTransactionResponse>
    {
        private readonly GiftCardDbContext _dbContext;
        private readonly IMapper _mapper;

        public DisableGiftCardCommandHandler(GiftCardDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<GiftCardTransactionResponse> Handle(DisableGiftCardCommand command, CancellationToken cancellationToken)
        {
            var giftCard = await _dbContext.GiftCards
                .FirstOrDefaultAsync(g => g.Code == command.Code, cancellationToken)
                ?? throw new InvalidOperationException("Gift card not found");

            if (giftCard.Status == "Disabled")
                throw new InvalidOperationException("Gift card is already disabled");

            var previousBalance = giftCard.CurrentBalance;
            giftCard.Status = "Disabled";
            giftCard.CurrentBalance = 0;
            giftCard.UpdatedAt = DateTime.UtcNow;

            var transaction = new GiftCardTransaction
            {
                GiftCardId = giftCard.Id,
                Type = "void",
                Amount = -previousBalance,
                BalanceAfter = 0,
                Description = $"Gift card disabled: {command.Reason}"
            };

            _dbContext.GiftCardTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return _mapper.Map<GiftCardTransactionResponse>(transaction);
        }
    }
}
