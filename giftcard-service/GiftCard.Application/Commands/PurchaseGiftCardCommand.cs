using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce.Events.GiftCard;
using Ecommerce.Model.GiftCard.Response;
using GiftCard.Application.Entities;
using MassTransit;
using MediatR;

namespace GiftCard.Application.Commands
{
    public class PurchaseGiftCardCommand : IRequest<GiftCardResponse>
    {
        public decimal Value { get; set; }
        public string? RecipientEmail { get; set; }
        public string? PersonalMessage { get; set; }
        public string PurchasedByCustomerId { get; set; } = string.Empty;
        public bool IsDigital { get; set; } = true;
    }

    public class PurchaseGiftCardCommandHandler : IRequestHandler<PurchaseGiftCardCommand, GiftCardResponse>
    {
        private readonly GiftCardDbContext _dbContext;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMapper _mapper;

        public PurchaseGiftCardCommandHandler(GiftCardDbContext dbContext, IPublishEndpoint publishEndpoint, IMapper mapper)
        {
            _dbContext = dbContext;
            _publishEndpoint = publishEndpoint;
            _mapper = mapper;
        }

        public async Task<GiftCardResponse> Handle(PurchaseGiftCardCommand command, CancellationToken cancellationToken)
        {
            if (command.Value <= 0)
                throw new InvalidOperationException("Gift card value must be greater than zero");

            var code = GenerateCode();

            var giftCard = new GiftCardEntity
            {
                Code = code,
                InitialValue = command.Value,
                CurrentBalance = command.Value,
                Status = "Active",
                RecipientEmail = command.RecipientEmail,
                PersonalMessage = command.PersonalMessage,
                PurchasedByCustomerId = command.PurchasedByCustomerId,
                IsDigital = command.IsDigital,
                ActivatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(2)
            };

            _dbContext.GiftCards.Add(giftCard);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var transaction = new GiftCardTransaction
            {
                GiftCardId = giftCard.Id,
                Type = "purchase",
                Amount = command.Value,
                BalanceAfter = giftCard.CurrentBalance,
                Description = $"Gift card purchased with value {command.Value:C}"
            };

            _dbContext.GiftCardTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _publishEndpoint.Publish(new GiftCardIssued
            {
                Code = code,
                Value = command.Value,
                RecipientEmail = command.RecipientEmail,
                PersonalMessage = command.PersonalMessage,
                PurchasedByCustomerId = command.PurchasedByCustomerId
            }, cancellationToken);

            return _mapper.Map<GiftCardResponse>(giftCard);
        }

        private static string GenerateCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            var code = new char[16];
            for (int i = 0; i < 16; i++)
            {
                code[i] = chars[bytes[i] % chars.Length];
            }

            return $"{new string(code, 0, 4)}-{new string(code, 4, 4)}-{new string(code, 8, 4)}-{new string(code, 12, 4)}";
        }
    }
}
