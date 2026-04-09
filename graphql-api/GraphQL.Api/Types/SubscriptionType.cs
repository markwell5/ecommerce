namespace GraphQL.Api.Types;

public class SubscriptionItem
{
    public long Id { get; set; }
    public string CustomerId { get; set; } = default!;
    public long ProductId { get; set; }
    public string ProductName { get; set; } = default!;
    public int Quantity { get; set; }
    public string Frequency { get; set; } = default!;
    public int IntervalDays { get; set; }
    public decimal DiscountPercent { get; set; }
    public string Status { get; set; } = default!;
    public long DeliveryAddressId { get; set; }
    public string NextRenewalAt { get; set; } = default!;
    public string? LastRenewedAt { get; set; }
    public int FailureCount { get; set; }
    public string CreatedAt { get; set; } = default!;
    public string UpdatedAt { get; set; } = default!;
}
