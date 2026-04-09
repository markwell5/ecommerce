using System.Globalization;
using Ecommerce.Model.Subscription.Response;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Subscription.Application.Commands;
using Subscription.Application.Queries;

namespace Subscription.Service.Services;

public class SubscriptionGrpcService : SubscriptionGrpc.SubscriptionGrpcBase
{
    private readonly IMediator _mediator;

    public SubscriptionGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<SubscriptionReply> GetSubscription(GetSubscriptionRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetSubscriptionQuery(request.Id), context.CancellationToken);
        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Subscription {request.Id} not found"));
        return MapToReply(result);
    }

    public override async Task<GetSubscriptionsByCustomerReply> GetSubscriptionsByCustomer(GetSubscriptionsByCustomerRequest request, ServerCallContext context)
    {
        var results = await _mediator.Send(new GetSubscriptionsByCustomerQuery(request.CustomerId), context.CancellationToken);
        var reply = new GetSubscriptionsByCustomerReply();
        foreach (var r in results) reply.Subscriptions.Add(MapToReply(r));
        return reply;
    }

    public override async Task<GetUpcomingRenewalsReply> GetUpcomingRenewals(GetUpcomingRenewalsRequest request, ServerCallContext context)
    {
        var results = await _mediator.Send(new GetUpcomingRenewalsQuery(request.Days), context.CancellationToken);
        var reply = new GetUpcomingRenewalsReply();
        foreach (var r in results) reply.Subscriptions.Add(MapToReply(r));
        return reply;
    }

    public override async Task<SubscriptionReply> CreateSubscription(CreateSubscriptionGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CreateSubscriptionCommand
        {
            CustomerId = request.CustomerId,
            ProductId = request.ProductId,
            ProductName = request.ProductName,
            Quantity = request.Quantity,
            Frequency = request.Frequency,
            IntervalDays = request.IntervalDays,
            DiscountPercent = decimal.TryParse(request.DiscountPercent, CultureInfo.InvariantCulture, out var d) ? d : 0,
            DeliveryAddressId = request.DeliveryAddressId
        }, context.CancellationToken);

        return MapToReply(result);
    }

    public override async Task<SubscriptionReply> UpdateSubscription(UpdateSubscriptionGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new UpdateSubscriptionCommand
        {
            Id = request.Id,
            Quantity = request.Quantity > 0 ? request.Quantity : null,
            Frequency = string.IsNullOrEmpty(request.Frequency) ? null : request.Frequency,
            IntervalDays = request.IntervalDays > 0 ? request.IntervalDays : null,
            DeliveryAddressId = request.DeliveryAddressId > 0 ? request.DeliveryAddressId : null
        }, context.CancellationToken);

        return MapToReply(result);
    }

    public override async Task<SubscriptionReply> PauseSubscription(SubscriptionActionRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new PauseSubscriptionCommand(request.Id), context.CancellationToken);
        return MapToReply(result);
    }

    public override async Task<SubscriptionReply> ResumeSubscription(SubscriptionActionRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new ResumeSubscriptionCommand(request.Id), context.CancellationToken);
        return MapToReply(result);
    }

    public override async Task<SubscriptionReply> CancelSubscription(SubscriptionActionRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CancelSubscriptionCommand(request.Id), context.CancellationToken);
        return MapToReply(result);
    }

    public override async Task<SubscriptionReply> SkipNextDelivery(SubscriptionActionRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new SkipNextDeliveryCommand(request.Id), context.CancellationToken);
        return MapToReply(result);
    }

    private static SubscriptionReply MapToReply(SubscriptionResponse s) => new()
    {
        Id = s.Id,
        CustomerId = s.CustomerId,
        ProductId = s.ProductId,
        ProductName = s.ProductName,
        Quantity = s.Quantity,
        Frequency = s.Frequency,
        IntervalDays = s.IntervalDays,
        DiscountPercent = s.DiscountPercent.ToString(CultureInfo.InvariantCulture),
        Status = s.Status,
        DeliveryAddressId = s.DeliveryAddressId,
        NextRenewalAt = s.NextRenewalAt.ToString("O"),
        LastRenewedAt = s.LastRenewedAt?.ToString("O") ?? string.Empty,
        FailureCount = s.FailureCount,
        CreatedAt = s.CreatedAt.ToString("O"),
        UpdatedAt = s.UpdatedAt.ToString("O")
    };
}
