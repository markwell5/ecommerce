using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Model.Discount.Response;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Order.Application.Commands;

namespace Order.Application.Queries
{
    public class GetCouponsQuery : IRequest<List<CouponResponse>>
    {
    }

    public class GetCouponsQueryHandler : IRequestHandler<GetCouponsQuery, List<CouponResponse>>
    {
        private readonly OrderDbContext _dbContext;

        public GetCouponsQueryHandler(OrderDbContext dbContext) { _dbContext = dbContext; }

        public async Task<List<CouponResponse>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
        {
            var coupons = await _dbContext.Coupons
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            return coupons.Select(CreateCouponCommandHandler.MapToResponse).ToList();
        }
    }
}
