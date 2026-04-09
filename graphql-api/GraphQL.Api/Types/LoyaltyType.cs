namespace GraphQL.Api.Types;

public class LoyaltyAccount
{
    public long Id { get; set; }
    public string CustomerId { get; set; } = default!;
    public int PointsBalance { get; set; }
    public int LifetimePoints { get; set; }
    public decimal AnnualSpend { get; set; }
    public string Tier { get; set; } = default!;
    public double PointsMultiplier { get; set; }
    public string? LastActivityAt { get; set; }
    public string TierExpiresAt { get; set; } = default!;
    public string CreatedAt { get; set; } = default!;
}

public class PointsTransaction
{
    public long Id { get; set; }
    public string CustomerId { get; set; } = default!;
    public string Type { get; set; } = default!;
    public int Points { get; set; }
    public int BalanceAfter { get; set; }
    public string Description { get; set; } = default!;
    public string? OrderId { get; set; }
    public string CreatedAt { get; set; } = default!;
}

public class PointsHistoryConnection
{
    public List<PointsTransaction> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
