using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Product.Application.Queries;

namespace Product.Service.Services;

public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IMediator _mediator;

    public ProductGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<ProductReply> GetProduct(GetProductRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetProductQuery(request.Id), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} not found"));

        return new ProductReply
        {
            Id = result.Id,
            Name = result.Name ?? string.Empty,
            Description = result.Description ?? string.Empty,
            Category = result.Category ?? string.Empty,
            Price = result.Price.ToString()
        };
    }

    public override async Task<GetProductsReply> GetProducts(GetProductsRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetProductsQuery
        {
            Page = request.Page,
            PageSize = request.PageSize
        }, context.CancellationToken);

        var reply = new GetProductsReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        foreach (var p in result.Items)
        {
            reply.Products.Add(new ProductReply
            {
                Id = p.Id,
                Name = p.Name ?? string.Empty,
                Description = p.Description ?? string.Empty,
                Category = p.Category ?? string.Empty,
                Price = p.Price.ToString()
            });
        }

        return reply;
    }
}
