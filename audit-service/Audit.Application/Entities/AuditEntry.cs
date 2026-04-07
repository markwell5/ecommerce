using System;

namespace Audit.Application.Entities
{
    public class AuditEntry
    {
        public long Id { get; set; }
        public string Service { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public string ActorType { get; set; } = "User";
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string BeforeState { get; set; } = string.Empty;
        public string AfterState { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string PreviousHash { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public DateTime IngestedAt { get; set; } = DateTime.UtcNow;
    }
}
