using System;

namespace Order.Application.Entities
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ItemsJson { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
