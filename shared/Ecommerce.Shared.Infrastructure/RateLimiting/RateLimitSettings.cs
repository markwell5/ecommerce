namespace Ecommerce.Shared.Infrastructure.RateLimiting;

public class RateLimitSettings
{
    public PolicySettings Read { get; set; } = new();
    public PolicySettings Write { get; set; } = new();
}

public class PolicySettings
{
    public int PermitLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
    public int SegmentsPerWindow { get; set; } = 6;
}
