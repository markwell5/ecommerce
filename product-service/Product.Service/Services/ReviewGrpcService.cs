using Ecommerce.Model.Review.Request;
using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Product.Application.Commands;
using Product.Application.Queries;

namespace Product.Service.Services;

public class ReviewGrpcService : ReviewGrpc.ReviewGrpcBase
{
    private readonly IMediator _mediator;

    public ReviewGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<GetProductReviewsReply> GetProductReviews(GetProductReviewsRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(
            new GetProductReviewsQuery(request.ProductId, request.Page, request.PageSize),
            context.CancellationToken);

        var reply = new GetProductReviewsReply
        {
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };

        foreach (var r in result.Items)
        {
            reply.Reviews.Add(new ReviewReply
            {
                Id = r.Id,
                ProductId = r.ProductId,
                CustomerId = r.CustomerId ?? string.Empty,
                Rating = r.Rating,
                Title = r.Title ?? string.Empty,
                Body = r.Body ?? string.Empty,
                CreatedAt = r.CreatedAt.ToString("O")
            });
        }

        return reply;
    }

    public override async Task<ProductRatingReply> GetProductRating(GetProductRatingRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(
            new GetProductRatingQuery(request.ProductId),
            context.CancellationToken);

        return new ProductRatingReply
        {
            ProductId = result.ProductId,
            AverageRating = result.AverageRating,
            ReviewCount = result.ReviewCount
        };
    }

    public override async Task<ReviewReply> CreateReview(CreateReviewGrpcRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new CreateReviewCommand(request.CustomerId, new CreateReviewRequest
        {
            ProductId = request.ProductId,
            Rating = request.Rating,
            Title = request.Title,
            Body = request.Body
        }), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.AlreadyExists, "Customer has already reviewed this product"));

        return new ReviewReply
        {
            Id = result.Id,
            ProductId = result.ProductId,
            CustomerId = result.CustomerId ?? string.Empty,
            Rating = result.Rating,
            Title = result.Title ?? string.Empty,
            Body = result.Body ?? string.Empty,
            CreatedAt = result.CreatedAt.ToString("O")
        };
    }
}
