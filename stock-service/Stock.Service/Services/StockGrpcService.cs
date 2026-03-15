using Ecommerce.Shared.Protos;
using Grpc.Core;
using MediatR;
using Stock.Application.Queries;

namespace Stock.Service.Services;

public class StockGrpcService : StockGrpc.StockGrpcBase
{
    private readonly IMediator _mediator;

    public StockGrpcService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async Task<StockLevelReply> GetStockLevel(GetStockLevelRequest request, ServerCallContext context)
    {
        var result = await _mediator.Send(new GetStockQuery(request.ProductId), context.CancellationToken);

        if (result is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Stock for product {request.ProductId} not found"));

        return new StockLevelReply
        {
            ProductId = result.ProductId,
            AvailableQuantity = result.AvailableQuantity,
            ReservedQuantity = result.ReservedQuantity
        };
    }
}
