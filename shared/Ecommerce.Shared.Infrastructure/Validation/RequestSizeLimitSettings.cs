namespace Ecommerce.Shared.Infrastructure.Validation;

public class RequestSizeLimitSettings
{
    public long MaxRequestBodySizeBytes { get; set; } = 1_048_576; // 1 MB default
}
