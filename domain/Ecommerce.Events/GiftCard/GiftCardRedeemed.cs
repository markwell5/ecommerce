namespace Ecommerce.Events.GiftCard
{
    public class GiftCardRedeemed
    {
        public string Code { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string? OrderId { get; init; }
        public decimal RemainingBalance { get; init; }
    }
}
