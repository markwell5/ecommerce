namespace Ecommerce.Shared.Infrastructure.Logging;

public class RequestResponseLoggingSettings
{
    public bool Enabled { get; set; }
    public int MaxBodyLength { get; set; } = 4096;
    public string[] SensitiveFields { get; set; } =
    {
        "password",
        "currentPassword",
        "newPassword",
        "token",
        "refreshToken",
        "accessToken",
        "secret",
        "secretKey",
        "cardNumber",
        "cvv",
        "cvc",
        "apiKey",
        "authorization"
    };
}
