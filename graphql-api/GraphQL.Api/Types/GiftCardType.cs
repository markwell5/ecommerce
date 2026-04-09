namespace GraphQL.Api.Types;

public class GiftCardItem
{
    public long Id { get; set; }
    public string Code { get; set; } = default!;
    public decimal InitialValue { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Status { get; set; } = default!;
    public string? RecipientEmail { get; set; }
    public string? PersonalMessage { get; set; }
    public string PurchasedByCustomerId { get; set; } = default!;
    public bool IsDigital { get; set; }
    public string? ActivatedAt { get; set; }
    public string? ExpiresAt { get; set; }
    public string CreatedAt { get; set; } = default!;
    public string UpdatedAt { get; set; } = default!;
}

public class GiftCardTransaction
{
    public long Id { get; set; }
    public long GiftCardId { get; set; }
    public string Type { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? OrderId { get; set; }
    public string Description { get; set; } = default!;
    public string CreatedAt { get; set; } = default!;
}

public class GiftCardTransactionConnection
{
    public List<GiftCardTransaction> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
