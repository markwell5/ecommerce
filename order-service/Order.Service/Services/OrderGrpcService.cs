using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Order.Application.Queries;

namespace Order.Service.Services;

public class OrderGrpcService : OrderGrpc.OrderGrpcBase
{
    private readonly IMediator _mediator;

    public OrderGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<OrderReply> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order ID format"));

        var result = await _mediator.Send(new GetOrderQuery(orderId), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Order {request.OrderId} not found"));

        return new OrderReply
        {
            OrderId = result.OrderId.ToString(),
            CustomerId = result.CustomerId ?? string.Empty,
            Status = result.Status ?? string.Empty,
            TotalAmount = result.TotalAmount.ToString(),
            ItemsJson = result.ItemsJson ?? string.Empty,
            CreatedAt = result.CreatedAt.ToString("O"),
            UpdatedAt = result.UpdatedAt?.ToString("O") ?? string.Empty
        };
    }
}
