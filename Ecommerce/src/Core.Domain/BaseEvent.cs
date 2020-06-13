using System;

namespace Core.Domain
{
    public class BaseEvent
    {
        public Guid EventId { get; set; } = Guid.NewGuid();
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
    }
}
