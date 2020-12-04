using System;

namespace Ecommerce.Events
{
    public interface IEvent
    {
        public string EventName { get; }
        public string IdempotencyKey { get; }
        public DateTime DateEmitted { get; }
    }
}
