using Cart.Application.Queries;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;

namespace Cart.Service.Services;

public class CartGrpcService : CartGrpc.CartGrpcBase
{
    private readonly IMediator _mediator;

    public CartGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<CartReply> GetCart(GetCartRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetCartQuery(request.CartId), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Cart {request.CartId} not found"));

        var reply = new CartReply
        {
            Id = result.Id,
            TotalPrice = result.TotalPrice.ToString(),
            LastModifiedAt = result.LastModifiedAt.ToString("O")
        };

        foreach (var item in result.Items)
        {
            reply.Items.Add(new CartItemReply
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName ?? string.Empty,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice.ToString(),
                LineTotal = item.LineTotal.ToString()
            });
        }

        return reply;
    }
}
