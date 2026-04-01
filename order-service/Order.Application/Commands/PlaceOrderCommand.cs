using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using Ecommerce.Model.Order.Request;
using Ecommerce.Model.Order.Response;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Order.Application.Commands
{
    public class PlaceOrderCommand : IRequest<OrderResponse>
    {
        public PlaceOrderCommand(PlaceOrderRequest request)
        {
            Request = request;
        }

        public PlaceOrderRequest Request { get; }
    }

    public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderResponse>
    {
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly OrderDbContext _dbContext;

        public PlaceOrderCommandHandler(ISendEndpointProvider sendEndpointProvider, OrderDbContext dbContext)
        {
            _sendEndpointProvider = sendEndpointProvider;
            _dbContext = dbContext;
        }

        public async Task<OrderResponse> Handle(PlaceOrderCommand command, CancellationToken cancellationToken)
        {
            var request = command.Request;
            var orderId = Guid.NewGuid();
            var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            var itemsJson = JsonSerializer.Serialize(request.Items);

            string couponCode = null;
            decimal discountAmount = 0;

            if (!string.IsNullOrWhiteSpace(request.CouponCode))
            {
                var code = request.CouponCode.ToUpperInvariant();
                var coupon = await _dbContext.Coupons
                    .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);

                if (coupon != null && coupon.IsActive
                    && coupon.ExpiresAt >= DateTime.UtcNow
                    && (coupon.MaxUses == 0 || coupon.CurrentUses < coupon.MaxUses)
                    && subtotal >= coupon.MinOrderAmount)
                {
                    discountAmount = coupon.DiscountType == "percentage"
                        ? Math.Round(subtotal * coupon.Value / 100, 2)
                        : Math.Min(coupon.Value, subtotal);

                    couponCode = coupon.Code;
                    coupon.CurrentUses++;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            var totalAmount = subtotal - discountAmount;

            var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri("queue:order-state-machine"));

            await endpoint.Send(new PlaceOrder
            {
                OrderId = orderId,
                CustomerId = request.CustomerId,
                TotalAmount = totalAmount,
                ItemsJson = itemsJson,
                CouponCode = couponCode,
                DiscountAmount = discountAmount
            }, cancellationToken);

            return new OrderResponse
            {
                OrderId = orderId,
                CustomerId = request.CustomerId,
                Status = "Placed",
                TotalAmount = totalAmount,
                ItemsJson = itemsJson,
                CouponCode = couponCode,
                DiscountAmount = discountAmount,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
