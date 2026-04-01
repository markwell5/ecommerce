using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Discount.Request;
using Ecommerce.Model.Discount.Response;
using MediatR;

namespace Order.Application.Commands
{
    public class UpdateCouponCommand : IRequest<CouponResponse>
    {
        public UpdateCouponCommand(long id, UpdateCouponRequest request) { Id = id; Request = request; }
        public long Id { get; }
        public UpdateCouponRequest Request { get; }
    }

    public class UpdateCouponCommandHandler : IRequestHandler<UpdateCouponCommand, CouponResponse>
    {
        private readonly OrderDbContext _dbContext;

        public UpdateCouponCommandHandler(OrderDbContext dbContext) { _dbContext = dbContext; }

        public async Task<CouponResponse> Handle(UpdateCouponCommand command, CancellationToken cancellationToken)
        {
            var coupon = await _dbContext.Coupons.FindAsync(new object[] { command.Id }, cancellationToken);
            if (coupon == null) return null;

            coupon.DiscountType = command.Request.DiscountType;
            coupon.Value = command.Request.Value;
            coupon.MinOrderAmount = command.Request.MinOrderAmount;
            coupon.MaxUses = command.Request.MaxUses;
            coupon.ExpiresAt = command.Request.ExpiresAt;
            coupon.IsActive = command.Request.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return CreateCouponCommandHandler.MapToResponse(coupon);
        }
    }
}
