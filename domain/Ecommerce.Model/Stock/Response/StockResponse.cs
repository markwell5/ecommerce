namespace Ecommerce.Model.Stock.Response
{
    public class StockResponse
    {
        public long ProductId { get; set; }
        public int AvailableQuantity { get; set; }
        public int ReservedQuantity { get; set; }
    }
}
