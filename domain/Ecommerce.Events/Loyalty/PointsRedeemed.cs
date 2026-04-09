namespace Ecommerce.Events.Loyalty
{
    public class PointsRedeemed
    {
        public string CustomerId { get; init; } = string.Empty;
        public int Points { get; init; }
        public decimal DiscountAmount { get; init; }
        public string? OrderId { get; init; }
    }
}
