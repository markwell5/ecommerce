using System;

namespace Ecommerce.Events.Audit
{
    public class AuditEntryCreated
    {
        public string Service { get; init; } = string.Empty;
        public string Action { get; init; } = string.Empty;
        public string ActorId { get; init; } = string.Empty;
        public string ActorType { get; init; } = "User"; // User, System
        public string EntityType { get; init; } = string.Empty;
        public string EntityId { get; init; } = string.Empty;
        public string BeforeState { get; init; } = string.Empty;
        public string AfterState { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = string.Empty;
        public string IpAddress { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}
