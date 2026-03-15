using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Payment.Application.Queries;

namespace Payment.Service.Services;

public class PaymentGrpcService : PaymentGrpc.PaymentGrpcBase
{
    private readonly IMediator _mediator;

    public PaymentGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<PaymentReply> GetPaymentByOrder(GetPaymentByOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order ID format"));

        var result = await _mediator.Send(new GetPaymentByOrderQuery(orderId), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Payment for order {request.OrderId} not found"));

        return new PaymentReply
        {
            Id = result.Id,
            OrderId = result.OrderId.ToString(),
            CustomerId = result.CustomerId ?? string.Empty,
            Amount = result.Amount.ToString(),
            Currency = result.Currency ?? string.Empty,
            Status = result.Status ?? string.Empty,
            StripePaymentIntentId = result.StripePaymentIntentId ?? string.Empty,
            CreatedAt = result.CreatedAt.ToString("O")
        };
    }

    public override async Task<GetPaymentsByCustomerReply> GetPaymentsByCustomer(GetPaymentsByCustomerRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetPaymentsByCustomerQuery(request.CustomerId), context.CancellationToken);

        var reply = new GetPaymentsByCustomerReply();
        foreach (var p in result)
        {
            reply.Payments.Add(new PaymentReply
            {
                Id = p.Id,
                OrderId = p.OrderId.ToString(),
                CustomerId = p.CustomerId ?? string.Empty,
                Amount = p.Amount.ToString(),
                Currency = p.Currency ?? string.Empty,
                Status = p.Status ?? string.Empty,
                StripePaymentIntentId = p.StripePaymentIntentId ?? string.Empty,
                CreatedAt = p.CreatedAt.ToString("O")
            });
        }

        return reply;
    }
}
