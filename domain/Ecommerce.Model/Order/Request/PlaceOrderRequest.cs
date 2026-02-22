using System.Collections.Generic;

namespace Ecommerce.Model.Order.Request
{
    public class PlaceOrderRequest
    {
        public string CustomerId { get; set; }
        public List<OrderLineItem> Items { get; set; } = new();
    }

    public class OrderLineItem
    {
        public long ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
