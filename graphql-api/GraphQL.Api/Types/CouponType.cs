namespace GraphQL.Api.Types;

public class Coupon
{
    public long Id { get; set; }
    public string Code { get; set; } = default!;
    public string DiscountType { get; set; } = default!;
    public decimal Value { get; set; }
    public decimal MinOrderAmount { get; set; }
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public string ExpiresAt { get; set; } = default!;
    public bool IsActive { get; set; }
    public string CreatedAt { get; set; } = default!;
}

public class DiscountValidation
{
    public bool IsValid { get; set; }
    public string? Error { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? DiscountType { get; set; }
    public string? CouponCode { get; set; }
}
