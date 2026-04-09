using System;

namespace Ecommerce.Model.GiftCard.Response
{
    public class GiftCardResponse
    {
        public long Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal InitialValue { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RecipientEmail { get; set; }
        public string? PersonalMessage { get; set; }
        public string PurchasedByCustomerId { get; set; } = string.Empty;
        public bool IsDigital { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
