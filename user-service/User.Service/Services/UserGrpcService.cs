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
            CreatedAt = result.CreatedAt.ToString("O"),
            Role = result.Role ?? "User"
        };
    }

    public override async Task<GetUsersReply> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetUsersQuery
        {
            Page = request.Page > 0 ? request.Page : 1,
            PageSize = request.PageSize > 0 ? request.PageSize : 20,
            Search = request.Search
        }, context.CancellationToken);

        var reply = new GetUsersReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        foreach (var user in result.Users)
        {
            reply.Users.Add(new UserReply
            {
                Id = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Phone = user.Phone ?? string.Empty,
                CreatedAt = user.CreatedAt.ToString("O"),
                Role = user.Role ?? "User"
            });
        }

        return reply;
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
