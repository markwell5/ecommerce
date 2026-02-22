namespace Stock.Application.Entities
{
    public class StockItem
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; }
    }
}
