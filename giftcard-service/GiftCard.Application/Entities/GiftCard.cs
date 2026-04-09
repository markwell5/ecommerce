using System;

namespace GiftCard.Application.Entities
{
    public class GiftCardEntity
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal InitialValue { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Status { get; set; } = "Active"; // Active, Disabled, Expired
        public string? RecipientEmail { get; set; }
        public string? PersonalMessage { get; set; }
        public string PurchasedByCustomerId { get; set; } = string.Empty;
        public bool IsDigital { get; set; } = true;
        public DateTime? ActivatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
