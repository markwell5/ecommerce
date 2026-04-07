using System;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Discount.Request;
using Ecommerce.Model.Discount.Response;
using MediatR;

namespace Order.Application.Commands
{
    public class CreateCouponCommand : IRequest<CouponResponse>
    {
        public CreateCouponCommand(CreateCouponRequest request) { Request = request; }
        public CreateCouponRequest Request { get; }
    }

    public class CreateCouponCommandHandler : IRequestHandler<CreateCouponCommand, CouponResponse>
    {
        private readonly OrderDbContext _dbContext;

        public CreateCouponCommandHandler(OrderDbContext dbContext) { _dbContext = dbContext; }

        public async Task<CouponResponse> Handle(CreateCouponCommand command, CancellationToken cancellationToken)
        {
            var coupon = new Entities.Coupon
            {
                Code = command.Request.Code.ToUpperInvariant(),
                DiscountType = command.Request.DiscountType,
                Value = command.Request.Value,
                MinOrderAmount = command.Request.MinOrderAmount,
                MaxUses = command.Request.MaxUses,
                ExpiresAt = command.Request.ExpiresAt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Coupons.Add(coupon);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapToResponse(coupon);
        }

        internal static CouponResponse MapToResponse(Entities.Coupon c) => new()
        {
            Id = c.Id,
            Code = c.Code,
            DiscountType = c.DiscountType,
            Value = c.Value,
            MinOrderAmount = c.MinOrderAmount,
            MaxUses = c.MaxUses,
            CurrentUses = c.CurrentUses,
            ExpiresAt = c.ExpiresAt,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }
}
