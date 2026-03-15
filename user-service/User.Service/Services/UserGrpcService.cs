using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using User.Application.Queries;

namespace User.Service.Services;

public class UserGrpcService : UserGrpc.UserGrpcBase
{
    private readonly IMediator _mediator;

    public UserGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<UserReply> GetUser(GetUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));

        var result = await _mediator.Send(new GetProfileQuery(userId), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} not found"));

        return new UserReply
        {
            Id = result.Id.ToString(),
            Email = result.Email ?? string.Empty,
            FirstName = result.FirstName ?? string.Empty,
            LastName = result.LastName ?? string.Empty,
            Phone = result.Phone ?? string.Empty,
            CreatedAt = result.CreatedAt.ToString("O")
        };
    }

    public override async Task<GetAddressesReply> GetAddresses(GetAddressesRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID format"));

        var result = await _mediator.Send(new GetAddressesQuery(userId), context.CancellationToken);

        var reply = new GetAddressesReply();
        foreach (var addr in result)
        {
            reply.Addresses.Add(new AddressReply
            {
                Id = addr.Id,
                Line1 = addr.Line1 ?? string.Empty,
                Line2 = addr.Line2 ?? string.Empty,
                City = addr.City ?? string.Empty,
                County = addr.County ?? string.Empty,
                PostCode = addr.PostCode ?? string.Empty,
                Country = addr.Country ?? string.Empty,
                IsDefault = addr.IsDefault
            });
        }

        return reply;
    }
}
