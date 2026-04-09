namespace Ecommerce.Events.Loyalty
{
    public class TierChanged
    {
        public string CustomerId { get; init; } = string.Empty;
        public string PreviousTier { get; init; } = string.Empty;
        public string NewTier { get; init; } = string.Empty;
    }
}
