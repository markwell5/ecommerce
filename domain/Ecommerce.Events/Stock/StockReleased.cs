using System;

namespace Ecommerce.Events.Stock
{
    public class StockReleased : EventBase
    {
        public Guid OrderId { get; init; }
    }
}
