using System;

namespace GiftCard.Application.Entities
{
    public class GiftCardTransaction
    {
        public long Id { get; set; }
        public long GiftCardId { get; set; }
        public string Type { get; set; } = string.Empty; // purchase, redeem, topup, void
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? OrderId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
