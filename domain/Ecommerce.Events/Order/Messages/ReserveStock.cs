using System;

namespace Ecommerce.Events.Order.Messages
{
    public record ReserveStock
    {
        public Guid OrderId { get; init; }
        public string ItemsJson { get; init; }
    }
}
