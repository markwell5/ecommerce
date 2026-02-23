using System.Net.Http.Json;
using System.Text.Json;
using Cart.Application.Interfaces;

namespace Cart.Infrastructure;

public class ProductCatalogClient : IProductCatalogClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductInfo?> GetProductAsync(long productId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/product/{productId}", cancellationToken);
        if (!response.IsSuccessStatusCode) return null;

        var product = await response.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions, cancellationToken);
        return product is null ? null : new ProductInfo(product.Id, product.Name, product.Price);
    }

    private record ProductResponse(long Id, string Name, decimal Price);
}
