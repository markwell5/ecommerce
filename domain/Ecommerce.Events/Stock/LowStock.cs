namespace Ecommerce.Events.Stock
{
    public class LowStock : EventBase
    {
        public long ProductId { get; init; }
        public int AvailableQuantity { get; init; }
    }
}
