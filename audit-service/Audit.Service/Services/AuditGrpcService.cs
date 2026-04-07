using System;
using System.Globalization;
using System.Threading.Tasks;
using Audit.Application.Queries;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;

namespace Audit.Service.Services;

public class AuditGrpcService : AuditGrpc.AuditGrpcBase
{
    private readonly IMediator _mediator;

    public AuditGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<SearchAuditEntriesReply> SearchAuditEntries(SearchAuditEntriesRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new SearchAuditEntriesQuery
        {
            ActorId = request.ActorId,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Service = request.Service,
            Action = request.Action,
            CorrelationId = request.CorrelationId,
            From = ParseDate(request.From),
            To = ParseDate(request.To),
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 20
        }, context.CancellationToken);

        var reply = new SearchAuditEntriesReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        foreach (var e in result.Items)
        {
            reply.Entries.Add(new AuditEntryReply
            {
                Id = e.Id,
                Service = e.Service,
                Action = e.Action,
                ActorId = e.ActorId,
                ActorType = e.ActorType,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                BeforeState = e.BeforeState ?? string.Empty,
                AfterState = e.AfterState ?? string.Empty,
                CorrelationId = e.CorrelationId,
                IpAddress = e.IpAddress ?? string.Empty,
                Hash = e.Hash,
                PreviousHash = e.PreviousHash ?? string.Empty,
                Timestamp = e.Timestamp.ToString("O")
            });
        }

        return reply;
    }

    private static DateTime? ParseDate(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt)
            ? dt.ToUniversalTime() : null;
    }
}
