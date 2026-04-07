using Ecommerce.Model.Order.Request;
using Ecommerce.Model.Order.Response;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Order.Application.Commands;
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

        return MapToReply(result);
    }

    public override async Task<GetOrdersReply> GetOrders(GetOrdersRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetOrdersQuery
        {
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 20,
            Status = request.Status
        }, context.CancellationToken);

        var reply = new GetOrdersReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        foreach (var order in result.Orders)
        {
            reply.Orders.Add(MapToReply(order));
        }

        return reply;
    }

    public override async Task<GetOrdersByCustomerReply> GetOrdersByCustomer(GetOrdersByCustomerRequest request, ServerCallContext context)
    {
        var results = await _mediator.Send(new GetOrdersByCustomerQuery(request.CustomerId), context.CancellationToken);

        var reply = new GetOrdersByCustomerReply();
        foreach (var order in results)
        {
            reply.Orders.Add(MapToReply(order));
        }

        return reply;
    }

    public override async Task<OrderReply> PlaceOrder(PlaceOrderGrpcRequest request, ServerCallContext context)
    {
        var orderRequest = new PlaceOrderRequest
        {
            CustomerId = request.CustomerId,
            Items = request.Items.Select(i => new OrderLineItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = decimal.TryParse(i.UnitPrice, out var p) ? p : 0
            }).ToList()
        };

        var result = await _mediator.Send(new PlaceOrderCommand(orderRequest), context.CancellationToken);
        return MapToReply(result);
    }

    public override async Task<OrderActionReply> CancelOrder(OrderActionRequest request, ServerCallContext context)
    {
        var orderId = ParseOrderId(request.OrderId);
        var result = await _mediator.Send(new CancelOrderCommand(orderId), context.CancellationToken);
        return new OrderActionReply { Success = result };
    }

    public override async Task<OrderActionReply> ShipOrder(OrderActionRequest request, ServerCallContext context)
    {
        var orderId = ParseOrderId(request.OrderId);
        var result = await _mediator.Send(new ShipOrderCommand(orderId), context.CancellationToken);
        return new OrderActionReply { Success = result };
    }

    public override async Task<OrderActionReply> DeliverOrder(OrderActionRequest request, ServerCallContext context)
    {
        var orderId = ParseOrderId(request.OrderId);
        var result = await _mediator.Send(new DeliverOrderCommand(orderId), context.CancellationToken);
        return new OrderActionReply { Success = result };
    }

    public override async Task<OrderActionReply> ReturnOrder(OrderActionRequest request, ServerCallContext context)
    {
        var orderId = ParseOrderId(request.OrderId);
        var result = await _mediator.Send(new ReturnOrderCommand(orderId), context.CancellationToken);
        return new OrderActionReply { Success = result };
    }

    private static Guid ParseOrderId(string orderId)
    {
        if (!Guid.TryParse(orderId, out var parsed))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order ID format"));
        return parsed;
    }

    private static OrderReply MapToReply(OrderResponse result) => new()
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
