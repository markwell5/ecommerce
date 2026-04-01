using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

    var settings = configuration.GetSection("SeederSettings");
    var productServiceUrl = settings["ProductServiceUrl"] ?? "http://localhost:5001";
    var stockServiceUrl = settings["StockServiceUrl"] ?? "http://localhost:5003";
    var batchSize = int.Parse(settings["BatchSize"] ?? "10");
    var delayMs = int.Parse(settings["DelayBetweenBatchesMs"] ?? "500");
    var defaultStock = int.Parse(settings["DefaultStockQuantity"] ?? "100");
    var maxRetries = int.Parse(settings["MaxHealthCheckRetries"] ?? "30");
    var healthCheckIntervalMs = int.Parse(settings["HealthCheckIntervalMs"] ?? "2000");

    using var httpClient = new HttpClient();

    // Wait for services to be healthy
    await WaitForService(httpClient, $"{productServiceUrl}/health", "Product Service", maxRetries, healthCheckIntervalMs);
    await WaitForService(httpClient, $"{stockServiceUrl}/health", "Stock Service", maxRetries, healthCheckIntervalMs);

    // Load product data
    var productsJson = await File.ReadAllTextAsync("data/products.json");
    var products = JsonSerializer.Deserialize<List<SeedProduct>>(productsJson, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    Log.Information("Loaded {Count} products to seed", products!.Count);

    // Check existing products to avoid duplicates
    var existingResponse = await httpClient.GetAsync($"{productServiceUrl}/product?pageSize=1000");
    var existingProducts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    if (existingResponse.IsSuccessStatusCode)
    {
        var existingData = await existingResponse.Content.ReadFromJsonAsync<PagedResult>(new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        if (existingData?.Items != null)
        {
            foreach (var item in existingData.Items)
            {
                if (item.TryGetProperty("name", out var nameProp))
                    existingProducts.Add(nameProp.GetString()!);
            }
        }
    }

    var newProducts = products.Where(p => !existingProducts.Contains(p.Name)).ToList();
    if (newProducts.Count == 0)
    {
        Log.Information("All products already exist, skipping product creation");
    }
    else
    {
        Log.Information("{Count} new products to create ({Existing} already exist)", newProducts.Count, products.Count - newProducts.Count);
    }

    // Seed products in batches
    var createdProductIds = new List<long>();
    for (var i = 0; i < newProducts.Count; i += batchSize)
    {
        var batch = newProducts.Skip(i).Take(batchSize).ToList();

        foreach (var product in batch)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync($"{productServiceUrl}/product", new
                {
                    name = product.Name,
                    description = product.Description,
                    category = product.Category,
                    price = product.Price
                });

                if (response.IsSuccessStatusCode)
                {
                    var created = await response.Content.ReadFromJsonAsync<JsonElement>();
                    var id = created.GetProperty("id").GetInt64();
                    createdProductIds.Add(id);
                    Log.Information("Created product: {Name} (ID: {Id})", product.Name, id);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Log.Warning("Failed to create product {Name}: {StatusCode} {Error}", product.Name, response.StatusCode, error);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating product {Name}", product.Name);
            }
        }

        if (i + batchSize < newProducts.Count)
        {
            Log.Information("Batch complete, waiting {Delay}ms...", delayMs);
            await Task.Delay(delayMs);
        }
    }

    // Wait for stock records to be created by ProductCreated consumer
    if (createdProductIds.Count > 0)
    {
        Log.Information("Waiting for stock records to be created...");
        await Task.Delay(2000);

        // Set stock levels
        foreach (var productId in createdProductIds)
        {
            try
            {
                var response = await httpClient.PutAsJsonAsync($"{stockServiceUrl}/api/stock/{productId}", new
                {
                    quantity = defaultStock
                });

                if (response.IsSuccessStatusCode)
                {
                    Log.Information("Set stock for product {ProductId}: {Quantity}", productId, defaultStock);
                }
                else
                {
                    Log.Warning("Failed to set stock for product {ProductId}: {StatusCode}", productId, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error setting stock for product {ProductId}", productId);
            }
        }
    }

    Log.Information("Seeding complete. Created {ProductCount} products with {StockQuantity} stock each",
        createdProductIds.Count, defaultStock);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Data seeder failed");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;

static async Task WaitForService(HttpClient client, string healthUrl, string serviceName, int maxRetries, int intervalMs)
{
    for (var i = 0; i < maxRetries; i++)
    {
        try
        {
            var response = await client.GetAsync(healthUrl);
            if (response.IsSuccessStatusCode)
            {
                Log.Information("{ServiceName} is healthy", serviceName);
                return;
            }
        }
        catch
        {
            // Service not yet available
        }

        Log.Information("Waiting for {ServiceName}... ({Attempt}/{MaxRetries})", serviceName, i + 1, maxRetries);
        await Task.Delay(intervalMs);
    }

    throw new Exception($"{serviceName} did not become healthy after {maxRetries} attempts");
}

record SeedProduct(string Name, string Description, decimal Price, string Category);

class PagedResult
{
    public List<JsonElement> Items { get; set; } = new();
}
