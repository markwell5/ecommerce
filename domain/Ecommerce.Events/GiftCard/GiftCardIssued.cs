namespace Ecommerce.Events.GiftCard
{
    public class GiftCardIssued
    {
        public string Code { get; init; } = string.Empty;
        public decimal Value { get; init; }
        public string? RecipientEmail { get; init; }
        public string? PersonalMessage { get; init; }
        public string PurchasedByCustomerId { get; init; } = string.Empty;
    }
}
