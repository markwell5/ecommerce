using System.Net;
using System.Net.Http.Json;
using Ecommerce.Model;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using FluentAssertions;

namespace Product.Integration.Tests;

public class ProductApiTests : IClassFixture<ProductServiceFactory>
{
    private readonly HttpClient _client;

    public ProductApiTests(ProductServiceFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateProduct_ReturnsCreated()
    {
        var request = new CreateProductRequest
        {
            Name = "Test Product",
            Description = "A test product",
            Category = "Testing",
            Price = 19.99m
        };

        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product.Should().NotBeNull();
        product!.Name.Should().Be("Test Product");
        product.Price.Should().Be(19.99m);
        product.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetProduct_AfterCreate_ReturnsProduct()
    {
        var request = new CreateProductRequest
        {
            Name = "Fetchable Product",
            Description = "Can be fetched",
            Category = "Testing",
            Price = 9.99m
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.GetAsync($"/api/v1/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product!.Name.Should().Be("Fetchable Product");
    }

    [Fact]
    public async Task GetProduct_NotFound_Returns404()
    {
        var response = await _client.GetAsync("/api/v1/products/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProduct_ReturnsUpdated()
    {
        var createRequest = new CreateProductRequest
        {
            Name = "Original Name",
            Description = "Original",
            Category = "Testing",
            Price = 10.00m
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Name",
            Description = "Updated",
            Category = "Testing",
            Price = 15.00m
        };
        var response = await _client.PutAsJsonAsync($"/api/v1/products/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ProductResponse>();
        updated!.Name.Should().Be("Updated Name");
        updated.Price.Should().Be(15.00m);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNoContent()
    {
        var request = new CreateProductRequest
        {
            Name = "Deletable",
            Description = "Will be deleted",
            Category = "Testing",
            Price = 5.00m
        };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/products", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var deleteResponse = await _client.DeleteAsync($"/api/v1/products/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProducts_ReturnsPaginatedList()
    {
        for (var i = 0; i < 3; i++)
        {
            await _client.PostAsJsonAsync("/api/v1/products", new CreateProductRequest
            {
                Name = $"Paginated Product {i}",
                Description = "For pagination test",
                Category = "Pagination",
                Price = 1.00m + i
            });
        }

        var response = await _client.GetAsync("/api/v1/products?page=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.Content.ReadFromJsonAsync<PagedResponse<ProductResponse>>();
        paged.Should().NotBeNull();
        paged!.PageSize.Should().Be(2);
        paged.Items.Should().HaveCountLessOrEqualTo(2);
    }
}
