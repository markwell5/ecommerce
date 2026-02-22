using System;
using System.Collections.Generic;

namespace Ecommerce.Model.Order.Response
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ItemsJson { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderEventResponse> Events { get; set; } = new();
    }

    public class OrderEventResponse
    {
        public Guid Id { get; set; }
        public string EventName { get; set; }
        public string Payload { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
