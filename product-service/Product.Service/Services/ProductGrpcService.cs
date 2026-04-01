using Ecommerce.Model.Product.Request;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Product.Application.Commands;
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

        return MapToReply(result);
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
            reply.Products.Add(MapToReply(p));
        }

        return reply;
    }

    public override async Task<ProductReply> CreateProduct(CreateProductGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CreateProductCommand(new CreateProductRequest
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = decimal.TryParse(request.Price, out var p) ? p : 0
        }), context.CancellationToken);

        return MapToReply(result);
    }

    public override async Task<ProductReply> UpdateProduct(UpdateProductGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new UpdateProductCommand(request.Id, new UpdateProductRequest
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = decimal.TryParse(request.Price, out var p) ? p : 0
        }), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Product {request.Id} not found"));

        return MapToReply(result);
    }

    public override async Task<DeleteProductGrpcReply> DeleteProduct(DeleteProductGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new DeleteProductCommand(request.Id), context.CancellationToken);

        return new DeleteProductGrpcReply { Success = result };
    }

    private static ProductReply MapToReply(Ecommerce.Model.Product.Response.ProductResponse result) => new()
    {
        Id = result.Id,
        Name = result.Name ?? string.Empty,
        Description = result.Description ?? string.Empty,
        Category = result.Category ?? string.Empty,
        Price = result.Price.ToString()
    };
}
