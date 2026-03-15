using System;
using System.Threading.Tasks;
using Ecommerce.Events.Order.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Payment.Application.Consumers;

namespace Payment.Application.Tests.Consumers;

public class ProcessPaymentFaultConsumerTests
{
    [Fact]
    public async Task Consume_LogsCriticalOnPermanentFailure()
    {
        var logger = Substitute.For<ILogger<ProcessPaymentFaultConsumer>>();
        var consumer = new ProcessPaymentFaultConsumer(logger);

        var orderId = Guid.NewGuid();
        var fault = new FaultMessage<ProcessPayment>
        {
            Message = new ProcessPayment
            {
                OrderId = orderId,
                Amount = 99.99m,
                CustomerId = "cust-1"
            },
            FaultedMessageId = Guid.NewGuid(),
            Exceptions = new[]
            {
                new FaultExceptionInfo("System.Exception", "Stripe timeout", "", null)
            }
        };

        var context = Substitute.For<ConsumeContext<Fault<ProcessPayment>>>();
        context.Message.Returns(fault);

        await consumer.Consume(context);

        logger.ReceivedWithAnyArgs(1).LogCritical(default, default(Exception), default, default);
    }

    private class FaultMessage<T> : Fault<T> where T : class
    {
        public Guid FaultId { get; set; } = Guid.NewGuid();
        public Guid? FaultedMessageId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public ExceptionInfo[] Exceptions { get; set; } = Array.Empty<ExceptionInfo>();
        public HostInfo Host { get; set; }
        public T Message { get; set; }
        public string[] FaultMessageTypes { get; set; } = Array.Empty<string>();
    }

    private class FaultExceptionInfo : ExceptionInfo
    {
        public FaultExceptionInfo(string exceptionType, string message, string stackTrace, ExceptionInfo innerException)
        {
            ExceptionType = exceptionType;
            Message = message;
            StackTrace = stackTrace;
            InnerException = innerException;
        }

        public string ExceptionType { get; }
        public ExceptionInfo InnerException { get; }
        public string Message { get; }
        public string Source { get; } = "";
        public string StackTrace { get; }
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
    }
}
