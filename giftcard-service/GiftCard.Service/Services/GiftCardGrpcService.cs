using System.Globalization;
using Ecommerce.Model.GiftCard.Response;
using Ecommerce.Shared.Protos;
using GiftCard.Application.Commands;
using GiftCard.Application.Queries;
using Grpc.Core;
using MediatR;

namespace GiftCard.Service.Services;

public class GiftCardGrpcService : GiftCardGrpc.GiftCardGrpcBase
{
    private readonly IMediator _mediator;

    public GiftCardGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<GiftCardReply> GetGiftCardByCode(GetGiftCardByCodeRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetGiftCardByCodeQuery(request.Code), context.CancellationToken);
        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Gift card with code {request.Code} not found"));
        return MapToGiftCardReply(result);
    }

    public override async Task<GiftCardListReply> GetGiftCardsByCustomer(GetGiftCardsByCustomerRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetGiftCardsByCustomerQuery(request.CustomerId), context.CancellationToken);

        var reply = new GiftCardListReply();
        foreach (var g in result) reply.GiftCards.Add(MapToGiftCardReply(g));
        return reply;
    }

    public override async Task<GiftCardTransactionHistoryReply> GetGiftCardTransactions(GetGiftCardTransactionsRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(
            new GetGiftCardTransactionsQuery(request.Code, request.Page, request.PageSize),
            context.CancellationToken);

        var reply = new GiftCardTransactionHistoryReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
        foreach (var t in result.Items) reply.Transactions.Add(MapToTransactionReply(t));
        return reply;
    }

    public override async Task<GiftCardReply> PurchaseGiftCard(PurchaseGiftCardGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new PurchaseGiftCardCommand
        {
            Value = decimal.TryParse(request.Value, CultureInfo.InvariantCulture, out var v) ? v : 0,
            RecipientEmail = string.IsNullOrEmpty(request.RecipientEmail) ? null : request.RecipientEmail,
            PersonalMessage = string.IsNullOrEmpty(request.PersonalMessage) ? null : request.PersonalMessage,
            PurchasedByCustomerId = request.PurchasedByCustomerId,
            IsDigital = request.IsDigital
        }, context.CancellationToken);

        return MapToGiftCardReply(result);
    }

    public override async Task<GiftCardTransactionReply> RedeemGiftCard(RedeemGiftCardGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new RedeemGiftCardCommand
        {
            Code = request.Code,
            Amount = decimal.TryParse(request.Amount, CultureInfo.InvariantCulture, out var a) ? a : 0,
            OrderId = string.IsNullOrEmpty(request.OrderId) ? null : request.OrderId
        }, context.CancellationToken);

        return MapToTransactionReply(result);
    }

    public override async Task<GiftCardTransactionReply> TopUpGiftCard(TopUpGiftCardGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new TopUpGiftCardCommand
        {
            Code = request.Code,
            Amount = decimal.TryParse(request.Amount, CultureInfo.InvariantCulture, out var a) ? a : 0
        }, context.CancellationToken);

        return MapToTransactionReply(result);
    }

    public override async Task<GiftCardTransactionReply> DisableGiftCard(DisableGiftCardGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new DisableGiftCardCommand
        {
            Code = request.Code,
            Reason = request.Reason
        }, context.CancellationToken);

        return MapToTransactionReply(result);
    }

    private static GiftCardReply MapToGiftCardReply(GiftCardResponse g) => new()
    {
        Id = g.Id,
        Code = g.Code,
        InitialValue = g.InitialValue.ToString(CultureInfo.InvariantCulture),
        CurrentBalance = g.CurrentBalance.ToString(CultureInfo.InvariantCulture),
        Status = g.Status,
        RecipientEmail = g.RecipientEmail ?? string.Empty,
        PersonalMessage = g.PersonalMessage ?? string.Empty,
        PurchasedByCustomerId = g.PurchasedByCustomerId,
        IsDigital = g.IsDigital,
        ActivatedAt = g.ActivatedAt?.ToString("O") ?? string.Empty,
        ExpiresAt = g.ExpiresAt?.ToString("O") ?? string.Empty,
        CreatedAt = g.CreatedAt.ToString("O"),
        UpdatedAt = g.UpdatedAt.ToString("O")
    };

    private static GiftCardTransactionReply MapToTransactionReply(GiftCardTransactionResponse t) => new()
    {
        Id = t.Id,
        GiftCardId = t.GiftCardId,
        Type = t.Type,
        Amount = t.Amount.ToString(CultureInfo.InvariantCulture),
        BalanceAfter = t.BalanceAfter.ToString(CultureInfo.InvariantCulture),
        OrderId = t.OrderId ?? string.Empty,
        Description = t.Description,
        CreatedAt = t.CreatedAt.ToString("O")
    };
}
