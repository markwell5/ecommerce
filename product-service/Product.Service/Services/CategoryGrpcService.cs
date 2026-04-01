using Ecommerce.Model.Category.Request;
using Ecommerce.Model.Category.Response;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Product.Application.Commands;
using Product.Application.Queries;

namespace Product.Service.Services;

public class CategoryGrpcService : CategoryGrpc.CategoryGrpcBase
{
    private readonly IMediator _mediator;

    public CategoryGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<GetCategoriesReply> GetCategories(GetCategoriesRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), context.CancellationToken);

        var reply = new GetCategoriesReply();
        foreach (var cat in result)
        {
            reply.Categories.Add(MapToReply(cat));
        }

        return reply;
    }

    public override async Task<CategoryReply> GetCategoryBySlug(GetCategoryBySlugRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(
            new GetCategoryBySlugQuery(request.Slug, 1, 1), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Category '{request.Slug}' not found"));

        return MapToReply(result.Category);
    }

    public override async Task<CategoryReply> CreateCategory(CreateCategoryGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CreateCategoryCommand(new CreateCategoryRequest
        {
            Name = request.Name,
            Slug = request.Slug,
            ParentId = request.ParentId == 0 ? null : request.ParentId
        }), context.CancellationToken);

        return MapToReply(result);
    }

    public override async Task<CategoryReply> UpdateCategory(UpdateCategoryGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(request.Id, new UpdateCategoryRequest
        {
            Name = request.Name,
            Slug = request.Slug,
            ParentId = request.ParentId == 0 ? null : request.ParentId
        }), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Category {request.Id} not found"));

        return MapToReply(result);
    }

    public override async Task<DeleteCategoryGrpcReply> DeleteCategory(DeleteCategoryGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new DeleteCategoryCommand(request.Id), context.CancellationToken);
        return new DeleteCategoryGrpcReply { Success = result };
    }

    public override async Task<ProductCategoryGrpcReply> AssignProductCategory(ProductCategoryGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(
            new AssignProductCategoryCommand(request.ProductId, request.CategoryId),
            context.CancellationToken);
        return new ProductCategoryGrpcReply { Success = result };
    }

    public override async Task<ProductCategoryGrpcReply> RemoveProductCategory(ProductCategoryGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(
            new RemoveProductCategoryCommand(request.ProductId, request.CategoryId),
            context.CancellationToken);
        return new ProductCategoryGrpcReply { Success = result };
    }

    private static CategoryReply MapToReply(CategoryResponse cat)
    {
        var reply = new CategoryReply
        {
            Id = cat.Id,
            Name = cat.Name ?? string.Empty,
            Slug = cat.Slug ?? string.Empty,
            ParentId = cat.ParentId ?? 0
        };

        foreach (var child in cat.Children ?? new())
        {
            reply.Children.Add(MapToReply(child));
        }

        return reply;
    }
}
