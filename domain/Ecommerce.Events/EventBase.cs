using System;

namespace Ecommerce.Events
{
    public abstract class EventBase : IEvent
    {
        public string EventName => GetType().Name;

        public string IdempotencyKey { get; private set; } = Guid.NewGuid().ToString();

        public DateTime DateEmitted { get; private set; } = DateTime.UtcNow;
    }
}
