using Ecommerce.Model.Wishlist.Response;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Wishlist.Application.Commands;
using Wishlist.Application.Queries;

namespace Wishlist.Service.Services;

public class WishlistGrpcService : WishlistGrpc.WishlistGrpcBase
{
    private readonly IMediator _mediator;

    public WishlistGrpcService(IMediator mediator) => _mediator = mediator;

    public override async Task<WishlistListReply> GetWishlistsByCustomer(GetWishlistsByCustomerRequest request, ServerCallContext context)
    {
        var results = await _mediator.Send(new GetWishlistsByCustomerQuery(request.CustomerId), context.CancellationToken);
        var reply = new WishlistListReply();
        foreach (var w in results) reply.Wishlists.Add(MapWishlist(w));
        return reply;
    }

    public override async Task<WishlistReply> GetWishlistByShareToken(GetWishlistByShareTokenRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetWishlistByShareTokenQuery(request.ShareToken), context.CancellationToken);
        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Shared wishlist with token {request.ShareToken} not found"));
        return MapWishlist(result);
    }

    public override async Task<WishlistReply> GetWishlist(GetWishlistRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetWishlistQuery(request.Id), context.CancellationToken);
        if (result == null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Wishlist {request.Id} not found"));
        return MapWishlist(result);
    }

    public override async Task<WishlistReply> CreateWishlist(CreateWishlistGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CreateWishlistCommand
        {
            CustomerId = request.CustomerId,
            Name = request.Name
        }, context.CancellationToken);

        return MapWishlist(result);
    }

    public override async Task<WishlistReply> RenameWishlist(RenameWishlistGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new RenameWishlistCommand
        {
            Id = request.Id,
            Name = request.Name
        }, context.CancellationToken);

        return MapWishlist(result);
    }

    public override async Task<WishlistDeleteReply> DeleteWishlist(DeleteWishlistGrpcRequest request, ServerCallContext context)
    {
        var success = await _mediator.Send(new DeleteWishlistCommand { Id = request.Id }, context.CancellationToken);
        return new WishlistDeleteReply { Success = success };
    }

    public override async Task<WishlistReply> AddWishlistItem(AddWishlistItemGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new AddWishlistItemCommand
        {
            WishlistId = request.WishlistId,
            ProductId = request.ProductId,
            CustomerId = request.CustomerId
        }, context.CancellationToken);

        return MapWishlist(result);
    }

    public override async Task<WishlistReply> RemoveWishlistItem(RemoveWishlistItemGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new RemoveWishlistItemCommand
        {
            WishlistId = request.WishlistId,
            ProductId = request.ProductId
        }, context.CancellationToken);

        return MapWishlist(result);
    }

    public override async Task<WishlistReply> ToggleWishlistVisibility(ToggleWishlistVisibilityGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new ToggleWishlistVisibilityCommand
        {
            Id = request.Id,
            IsPublic = request.IsPublic
        }, context.CancellationToken);

        return MapWishlist(result);
    }

    public override async Task<WishlistReply> SetRestockNotification(SetRestockNotificationGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new SetRestockNotificationCommand
        {
            WishlistId = request.WishlistId,
            ProductId = request.ProductId,
            NotifyOnRestock = request.Notify
        }, context.CancellationToken);

        return MapWishlist(result);
    }

    private static WishlistReply MapWishlist(WishlistResponse w)
    {
        var reply = new WishlistReply
        {
            Id = w.Id,
            CustomerId = w.CustomerId,
            Name = w.Name,
            IsDefault = w.IsDefault,
            ShareToken = w.ShareToken.ToString(),
            IsPublic = w.IsPublic,
            CreatedAt = w.CreatedAt.ToString("O"),
            UpdatedAt = w.UpdatedAt.ToString("O")
        };

        foreach (var item in w.Items)
        {
            reply.Items.Add(new WishlistItemReply
            {
                Id = item.Id,
                WishlistId = item.WishlistId,
                ProductId = item.ProductId,
                NotifyOnRestock = item.NotifyOnRestock,
                AddedAt = item.AddedAt.ToString("O")
            });
        }

        return reply;
    }
}
