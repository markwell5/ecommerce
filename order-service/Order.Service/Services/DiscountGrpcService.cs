using System;
using Ecommerce.Model.Discount.Request;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Order.Application.Commands;
using Order.Application.Queries;

namespace Order.Service.Services;

public class DiscountGrpcService : DiscountGrpc.DiscountGrpcBase
{
    private readonly IMediator _mediator;

    public DiscountGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<GetCouponsReply> GetCoupons(GetCouponsRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetCouponsQuery(), context.CancellationToken);
        var reply = new GetCouponsReply();
        foreach (var c in result)
        {
            reply.Coupons.Add(MapToReply(c));
        }
        return reply;
    }

    public override async Task<DiscountValidationReply> ValidateDiscount(ValidateDiscountGrpcRequest request, ServerCallContext context)
    {
        var orderAmount = decimal.TryParse(request.OrderAmount, out var a) ? a : 0;
        var result = await _mediator.Send(
            new ValidateDiscountQuery(request.CouponCode, orderAmount),
            context.CancellationToken);

        return new DiscountValidationReply
        {
            IsValid = result.IsValid,
            Error = result.Error ?? string.Empty,
            DiscountAmount = result.DiscountAmount.ToString(),
            DiscountType = result.DiscountType ?? string.Empty,
            CouponCode = result.CouponCode ?? string.Empty
        };
    }

    public override async Task<CouponReply> CreateCoupon(CreateCouponGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CreateCouponCommand(new CreateCouponRequest
        {
            Code = request.Code,
            DiscountType = request.DiscountType,
            Value = decimal.TryParse(request.Value, out var v) ? v : 0,
            MinOrderAmount = decimal.TryParse(request.MinOrderAmount, out var m) ? m : 0,
            MaxUses = request.MaxUses,
            ExpiresAt = DateTime.TryParse(request.ExpiresAt, out var e) ? e.ToUniversalTime() : DateTime.UtcNow.AddYears(1)
        }), context.CancellationToken);

        return MapToReply(result);
    }

    public override async Task<CouponReply> UpdateCoupon(UpdateCouponGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new UpdateCouponCommand(request.Id, new UpdateCouponRequest
        {
            DiscountType = request.DiscountType,
            Value = decimal.TryParse(request.Value, out var v) ? v : 0,
            MinOrderAmount = decimal.TryParse(request.MinOrderAmount, out var m) ? m : 0,
            MaxUses = request.MaxUses,
            ExpiresAt = DateTime.TryParse(request.ExpiresAt, out var e) ? e.ToUniversalTime() : DateTime.UtcNow.AddYears(1),
            IsActive = request.IsActive
        }), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Coupon {request.Id} not found"));

        return MapToReply(result);
    }

    private static CouponReply MapToReply(Ecommerce.Model.Discount.Response.CouponResponse c) => new()
    {
        Id = c.Id,
        Code = c.Code ?? string.Empty,
        DiscountType = c.DiscountType ?? string.Empty,
        Value = c.Value.ToString(),
        MinOrderAmount = c.MinOrderAmount.ToString(),
        MaxUses = c.MaxUses,
        CurrentUses = c.CurrentUses,
        ExpiresAt = c.ExpiresAt.ToString("O"),
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt.ToString("O")
    };
}
