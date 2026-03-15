using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Shared.Infrastructure.Messaging;

public class FaultLoggingObserver : IConsumeObserver
{
    private readonly ILogger<FaultLoggingObserver> _logger;

    public FaultLoggingObserver(ILogger<FaultLoggingObserver> logger)
    {
        _logger = logger;
    }

    public Task PreConsume<T>(ConsumeContext<T> context) where T : class
    {
        return Task.CompletedTask;
    }

    public Task PostConsume<T>(ConsumeContext<T> context) where T : class
    {
        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
    {
        _logger.LogError(
            exception,
            "Consumer fault on {MessageType} (MessageId: {MessageId}, RetryAttempt: {RetryAttempt})",
            typeof(T).Name,
            context.MessageId,
            context.GetRetryAttempt());

        return Task.CompletedTask;
    }
}
