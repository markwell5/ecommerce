using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Discount.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Order.Application.Queries
{
    public class ValidateDiscountQuery : IRequest<DiscountValidationResponse>
    {
        public ValidateDiscountQuery(string couponCode, decimal orderAmount)
        {
            CouponCode = couponCode;
            OrderAmount = orderAmount;
        }

        public string CouponCode { get; }
        public decimal OrderAmount { get; }
    }

    public class ValidateDiscountQueryHandler : IRequestHandler<ValidateDiscountQuery, DiscountValidationResponse>
    {
        private readonly OrderDbContext _dbContext;

        public ValidateDiscountQueryHandler(OrderDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DiscountValidationResponse> Handle(ValidateDiscountQuery request, CancellationToken cancellationToken)
        {
            var code = request.CouponCode?.ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(code))
                return new DiscountValidationResponse { IsValid = false, Error = "Coupon code is required" };

            var coupon = await _dbContext.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

            if (coupon == null)
                return new DiscountValidationResponse { IsValid = false, Error = "Coupon not found" };

            if (!coupon.IsActive)
                return new DiscountValidationResponse { IsValid = false, Error = "Coupon is inactive" };

            if (coupon.ExpiresAt < DateTime.UtcNow)
                return new DiscountValidationResponse { IsValid = false, Error = "Coupon has expired" };

            if (coupon.MaxUses > 0 && coupon.CurrentUses >= coupon.MaxUses)
                return new DiscountValidationResponse { IsValid = false, Error = "Coupon has reached maximum uses" };

            if (request.OrderAmount < coupon.MinOrderAmount)
                return new DiscountValidationResponse { IsValid = false, Error = $"Minimum order amount is {coupon.MinOrderAmount}" };

            var discountAmount = coupon.DiscountType == "percentage"
                ? Math.Round(request.OrderAmount * coupon.Value / 100, 2)
                : Math.Min(coupon.Value, request.OrderAmount);

            return new DiscountValidationResponse
            {
                IsValid = true,
                DiscountAmount = discountAmount,
                DiscountType = coupon.DiscountType,
                CouponCode = coupon.Code
            };
        }
    }
}
