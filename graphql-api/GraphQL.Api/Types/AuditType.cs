namespace GraphQL.Api.Types;

public class AuditEntryItem
{
    public long Id { get; set; }
    public string Service { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string ActorId { get; set; } = default!;
    public string ActorType { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string BeforeState { get; set; } = default!;
    public string AfterState { get; set; } = default!;
    public string CorrelationId { get; set; } = default!;
    public string IpAddress { get; set; } = default!;
    public string Hash { get; set; } = default!;
    public string Timestamp { get; set; } = default!;
}

public class AuditConnection
{
    public List<AuditEntryItem> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
