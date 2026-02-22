using System;

namespace Order.Application.Entities
{
    public class OrderEvent
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string EventName { get; set; }
        public string Payload { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
