using Cart.Application.Interfaces;
using Ecommerce.Shared.Protos;
using Grpc.Core;

namespace Cart.Infrastructure;

public class ProductCatalogClient : IProductCatalogClient
{
    private readonly ProductGrpc.ProductGrpcClient _grpcClient;

    public ProductCatalogClient(ProductGrpc.ProductGrpcClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    public async Task<ProductInfo?> GetProductAsync(long productId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reply = await _grpcClient.GetProductAsync(
                new GetProductRequest { Id = productId },
                cancellationToken: cancellationToken);

            return new ProductInfo(reply.Id, reply.Name, decimal.Parse(reply.Price));
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            return null;
        }
    }
}
