using System;

namespace Ecommerce.Events.Order.Messages
{
    public record ReleaseStock
    {
        public Guid OrderId { get; init; }
        public string ItemsJson { get; init; }
    }
}
