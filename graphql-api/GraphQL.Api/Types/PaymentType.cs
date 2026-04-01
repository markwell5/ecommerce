namespace GraphQL.Api.Types;

public class Payment
{
    public long Id { get; set; }
    public string OrderId { get; set; } = default!;
    public string CustomerId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string StripePaymentIntentId { get; set; } = default!;
    public string CreatedAt { get; set; } = default!;
}
