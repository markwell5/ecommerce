namespace Ecommerce.Events.Stock
{
    public class StockUpdated : EventBase
    {
        public long ProductId { get; init; }
        public int AvailableQuantity { get; init; }
    }
}
