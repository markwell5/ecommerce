using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Stripe;

namespace Payment.Application.Services
{
    public class ResilientPaymentGateway : IPaymentGateway
    {
        private readonly IPaymentGateway _inner;
        private readonly ILogger<ResilientPaymentGateway> _logger;
        private readonly ResiliencePipeline _pipeline;

        public ResilientPaymentGateway(
            IPaymentGateway inner,
            ILogger<ResilientPaymentGateway> logger)
        {
            _inner = inner;
            _logger = logger;

            _pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromMilliseconds(500),
                    ShouldHandle = new PredicateBuilder().Handle<StripeException>(ex =>
                        ex.HttpStatusCode is
                            System.Net.HttpStatusCode.TooManyRequests or
                            System.Net.HttpStatusCode.ServiceUnavailable or
                            System.Net.HttpStatusCode.GatewayTimeout or
                            System.Net.HttpStatusCode.RequestTimeout),
                    OnRetry = args =>
                    {
                        _logger.LogWarning(
                            args.Outcome.Exception,
                            "Stripe call failed, retrying (attempt {AttemptNumber}/{MaxRetries})",
                            args.AttemptNumber,
                            3);
                        return ValueTask.CompletedTask;
                    }
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 5,
                    BreakDuration = TimeSpan.FromSeconds(30),
                    ShouldHandle = new PredicateBuilder().Handle<StripeException>(),
                    OnOpened = args =>
                    {
                        _logger.LogError("Circuit breaker opened for Stripe calls");
                        return ValueTask.CompletedTask;
                    },
                    OnClosed = args =>
                    {
                        _logger.LogInformation("Circuit breaker closed for Stripe calls");
                        return ValueTask.CompletedTask;
                    }
                })
                .AddTimeout(TimeSpan.FromSeconds(10))
                .Build();
        }

        public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
            decimal amount, string currency, Dictionary<string, string> metadata)
        {
            return await _pipeline.ExecuteAsync(async ct =>
                await _inner.CreatePaymentIntentAsync(amount, currency, metadata));
        }

        public async Task<RefundResult> CreateRefundAsync(string paymentIntentId, decimal amount)
        {
            return await _pipeline.ExecuteAsync(async ct =>
                await _inner.CreateRefundAsync(paymentIntentId, amount));
        }
    }
}
