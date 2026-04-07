using System;
using System.Globalization;
using System.Threading.Tasks;
using Analytics.Application.Queries;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;

namespace Analytics.Service.Services;

public class AnalyticsGrpcService : AnalyticsGrpc.AnalyticsGrpcBase
{
    private readonly IMediator _mediator;

    public AnalyticsGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<SalesOverviewReply> GetSalesOverview(GetSalesOverviewRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetSalesOverviewQuery
        {
            From = ParseDate(request.From),
            To = ParseDate(request.To)
        }, context.CancellationToken);

        return new SalesOverviewReply
        {
            TotalRevenue = result.TotalRevenue.ToString(CultureInfo.InvariantCulture),
            OrderCount = result.OrderCount,
            AvgOrderValue = result.AvgOrderValue.ToString(CultureInfo.InvariantCulture),
            CancelledCount = result.CancelledCount,
            ReturnedCount = result.ReturnedCount,
            NewCustomerCount = result.NewCustomerCount
        };
    }

    public override async Task<GetOrderStatusBreakdownReply> GetOrderStatusBreakdown(GetOrderStatusBreakdownRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetOrderStatusBreakdownQuery
        {
            From = ParseDate(request.From),
            To = ParseDate(request.To)
        }, context.CancellationToken);

        var reply = new GetOrderStatusBreakdownReply();
        foreach (var s in result)
        {
            reply.Statuses.Add(new StatusCountReply { Status = s.Status, Count = s.Count });
        }
        return reply;
    }

    public override async Task<GetDailyRevenueReply> GetDailyRevenue(GetDailyRevenueRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetDailyRevenueQuery
        {
            From = ParseDate(request.From),
            To = ParseDate(request.To)
        }, context.CancellationToken);

        var reply = new GetDailyRevenueReply();
        foreach (var p in result)
        {
            reply.Points.Add(new DailyRevenuePointReply
            {
                Date = p.Date,
                Revenue = p.Revenue.ToString(CultureInfo.InvariantCulture),
                OrderCount = p.OrderCount
            });
        }
        return reply;
    }

    private static DateTime? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToUniversalTime()
            : null;
    }
}
