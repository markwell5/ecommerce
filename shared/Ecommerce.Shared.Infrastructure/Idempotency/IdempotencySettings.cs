namespace Ecommerce.Shared.Infrastructure.Idempotency;

public class IdempotencySettings
{
    public int TtlHours { get; set; } = 24;
}
