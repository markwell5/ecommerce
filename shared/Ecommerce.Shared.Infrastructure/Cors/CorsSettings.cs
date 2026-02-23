namespace Ecommerce.Shared.Infrastructure.Cors;

public class CorsSettings
{
    public string[] AllowedOrigins { get; set; } = [];
    public string[] AllowedMethods { get; set; } = ["GET", "POST", "PUT", "DELETE", "OPTIONS"];
    public string[] AllowedHeaders { get; set; } = ["Content-Type", "Authorization", "X-Correlation-Id"];
}
