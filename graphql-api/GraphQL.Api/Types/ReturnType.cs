namespace GraphQL.Api.Types;

public class ReturnRequest
{
    public long Id { get; set; }
    public string RmaNumber { get; set; } = default!;
    public string OrderId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public long ProductId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string Resolution { get; set; } = default!;
    public decimal RefundAmount { get; set; }
    public decimal RestockingFee { get; set; }
    public string InspectionNotes { get; set; } = default!;
    public string AdminNotes { get; set; } = default!;
    public bool AutoApproved { get; set; }
    public string CreatedAt { get; set; } = default!;
    public string? ApprovedAt { get; set; }
    public string? ReceivedAt { get; set; }
    public string? ResolvedAt { get; set; }
    public long? ExchangeProductId { get; set; }
    public string? ExchangeProductName { get; set; }
    public string? ExchangeOrderId { get; set; }
}
