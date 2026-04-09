using System.Globalization;
using Ecommerce.Model.Loyalty.Response;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Loyalty.Application.Commands;
using Loyalty.Application.Queries;

namespace Loyalty.Service.Services;

public class LoyaltyGrpcService : LoyaltyGrpc.LoyaltyGrpcBase
{
    private readonly IMediator _mediator;

    public LoyaltyGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<LoyaltyAccountReply> GetLoyaltyAccount(GetLoyaltyAccountRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetLoyaltyAccountQuery(request.CustomerId), context.CancellationToken);
        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Loyalty account for customer {request.CustomerId} not found"));
        return MapToAccountReply(result);
    }

    public override async Task<PointsHistoryReply> GetPointsHistory(GetPointsHistoryRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(
            new GetPointsHistoryQuery(request.CustomerId, request.Page, request.PageSize),
            context.CancellationToken);

        var reply = new PointsHistoryReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        foreach (var t in result.Items) reply.Transactions.Add(MapToTransactionReply(t));
        return reply;
    }

    public override async Task<PointsTransactionReply> CreditPoints(CreditPointsGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CreditPointsCommand
        {
            CustomerId = request.CustomerId,
            Points = request.Points,
            Description = request.Description,
            OrderId = string.IsNullOrEmpty(request.OrderId) ? null : request.OrderId
        }, context.CancellationToken);

        return MapToTransactionReply(result);
    }

    public override async Task<PointsTransactionReply> RedeemPoints(RedeemPointsGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new RedeemPointsCommand
        {
            CustomerId = request.CustomerId,
            Points = request.Points,
            OrderId = string.IsNullOrEmpty(request.OrderId) ? null : request.OrderId
        }, context.CancellationToken);

        return MapToTransactionReply(result);
    }

    private static LoyaltyAccountReply MapToAccountReply(LoyaltyAccountResponse a) => new()
    {
        Id = a.Id,
        CustomerId = a.CustomerId,
        PointsBalance = a.PointsBalance,
        LifetimePoints = a.LifetimePoints,
        AnnualSpend = a.AnnualSpend.ToString(CultureInfo.InvariantCulture),
        Tier = a.Tier,
        PointsMultiplier = a.PointsMultiplier.ToString(CultureInfo.InvariantCulture),
        LastActivityAt = a.LastActivityAt?.ToString("O") ?? string.Empty,
        TierExpiresAt = a.TierExpiresAt.ToString("O"),
        CreatedAt = a.CreatedAt.ToString("O")
    };

    private static PointsTransactionReply MapToTransactionReply(PointsTransactionResponse t) => new()
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        Type = t.Type,
        Points = t.Points,
        BalanceAfter = t.BalanceAfter,
        Description = t.Description,
        OrderId = t.OrderId ?? string.Empty,
        CreatedAt = t.CreatedAt.ToString("O")
    };
}
