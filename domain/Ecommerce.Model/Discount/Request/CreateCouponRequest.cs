using System;

namespace Ecommerce.Model.Discount.Request
{
    public class CreateCouponRequest
    {
        public string Code { get; set; }
        public string DiscountType { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int MaxUses { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    public class UpdateCouponRequest
    {
        public string DiscountType { get; set; }
        public decimal Value { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int MaxUses { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class ValidateDiscountRequest
    {
        public string CouponCode { get; set; }
        public decimal OrderAmount { get; set; }
    }
}
