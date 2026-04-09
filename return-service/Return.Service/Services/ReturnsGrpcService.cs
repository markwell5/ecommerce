using System;
using System.Globalization;
using System.Threading.Tasks;
using Ecommerce.Model.Return.Response;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Return.Application.Commands;
using Return.Application.Queries;

namespace Return.Service.Services;

public class ReturnsGrpcService : ReturnsGrpc.ReturnsGrpcBase
{
    private readonly IMediator _mediator;

    public ReturnsGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<ReturnReply> GetReturn(GetReturnRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetReturnQuery(request.Id), context.CancellationToken);
        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Return {request.Id} not found"));
        return MapToReply(result);
    }

    public override async Task<GetReturnsByOrderReply> GetReturnsByOrder(GetReturnsByOrderRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid order ID format"));

        var results = await _mediator.Send(new GetReturnsByOrderQuery(orderId), context.CancellationToken);
        var reply = new GetReturnsByOrderReply();
        foreach (var r in results) reply.Returns.Add(MapToReply(r));
        return reply;
    }

    public override async Task<GetReturnsByCustomerReply> GetReturnsByCustomer(GetReturnsByCustomerRequest request, ServerCallContext context)
    {
        var results = await _mediator.Send(new GetReturnsByCustomerQuery(request.CustomerId), context.CancellationToken);
        var reply = new GetReturnsByCustomerReply();
        foreach (var r in results) reply.Returns.Add(MapToReply(r));
        return reply;
    }

    public override async Task<ReturnReply> CreateReturn(CreateReturnGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CreateReturnCommand
        {
            OrderId = Guid.Parse(request.OrderId),
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Reason = request.Reason,
            DeliveredAt = DateTime.TryParse(request.DeliveredAt, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal, out var dt) ? dt.ToUniversalTime() : DateTime.UtcNow
        }, context.CancellationToken);

        return MapToReply(result);
    }

    public override async Task<ReturnReply> ApproveReturn(ApproveReturnGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new ApproveReturnCommand(request.Id, request.AdminNotes), context.CancellationToken);
        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Return {request.Id} not found"));
        return MapToReply(result);
    }

    public override async Task<ReturnReply> RejectReturn(RejectReturnGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new RejectReturnCommand(request.Id, request.AdminNotes), context.CancellationToken);
        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Return {request.Id} not found"));
        return MapToReply(result);
    }

    public override async Task<ReturnReply> ResolveReturn(ResolveReturnGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new ResolveReturnCommand
        {
            ReturnRequestId = request.Id,
            Resolution = request.Resolution,
            RefundAmount = decimal.TryParse(request.RefundAmount, CultureInfo.InvariantCulture, out var a) ? a : 0,
            ExchangeProductId = request.ExchangeProductId == 0 ? null : request.ExchangeProductId,
            ExchangeProductName = request.ExchangeProductName
        }, context.CancellationToken);

        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Return {request.Id} not found"));
        return MapToReply(result);
    }

    private static ReturnReply MapToReply(ReturnResponse r) => new()
    {
        Id = r.Id,
        RmaNumber = r.RmaNumber,
        OrderId = r.OrderId.ToString(),
        CustomerId = r.CustomerId,
        ProductId = r.ProductId,
        Quantity = r.Quantity,
        Reason = r.Reason,
        Status = r.Status,
        Resolution = r.Resolution ?? string.Empty,
        RefundAmount = r.RefundAmount.ToString(CultureInfo.InvariantCulture),
        RestockingFee = r.RestockingFee.ToString(CultureInfo.InvariantCulture),
        InspectionNotes = r.InspectionNotes ?? string.Empty,
        AdminNotes = r.AdminNotes ?? string.Empty,
        AutoApproved = r.AutoApproved,
        CreatedAt = r.CreatedAt.ToString("O"),
        ApprovedAt = r.ApprovedAt?.ToString("O") ?? string.Empty,
        ReceivedAt = r.ReceivedAt?.ToString("O") ?? string.Empty,
        ResolvedAt = r.ResolvedAt?.ToString("O") ?? string.Empty,
        ExchangeProductId = r.ExchangeProductId ?? 0,
        ExchangeProductName = r.ExchangeProductName ?? string.Empty,
        ExchangeOrderId = r.ExchangeOrderId?.ToString() ?? string.Empty
    };
}
