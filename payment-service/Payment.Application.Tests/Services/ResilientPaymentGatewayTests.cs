using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Payment.Application.Services;
using Stripe;

namespace Payment.Application.Tests.Services;

public class ResilientPaymentGatewayTests
{
    private readonly IPaymentGateway _innerGateway;
    private readonly ILogger<ResilientPaymentGateway> _logger;

    public ResilientPaymentGatewayTests()
    {
        _innerGateway = Substitute.For<IPaymentGateway>();
        _logger = Substitute.For<ILogger<ResilientPaymentGateway>>();
    }

    [Fact]
    public async Task CreatePaymentIntent_RetriesOnTransientStripeError()
    {
        var callCount = 0;
        _innerGateway.CreatePaymentIntentAsync(
            Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
            .ReturnsForAnyArgs(callInfo =>
            {
                callCount++;
                if (callCount <= 2)
                    throw new StripeException(HttpStatusCode.ServiceUnavailable, null, "Service unavailable");

                return new PaymentIntentResult
                {
                    PaymentIntentId = "pi_test",
                    Status = "succeeded"
                };
            });

        var gateway = new ResilientPaymentGateway(_innerGateway, _logger);

        var result = await gateway.CreatePaymentIntentAsync(
            100m, "usd", new Dictionary<string, string>());

        result.PaymentIntentId.Should().Be("pi_test");
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task CreatePaymentIntent_ThrowsAfterMaxRetries()
    {
        _innerGateway.CreatePaymentIntentAsync(
            Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
            .ThrowsAsyncForAnyArgs(new StripeException(HttpStatusCode.ServiceUnavailable, null, "Service unavailable"));

        var gateway = new ResilientPaymentGateway(_innerGateway, _logger);

        var act = () => gateway.CreatePaymentIntentAsync(
            100m, "usd", new Dictionary<string, string>());

        await act.Should().ThrowAsync<StripeException>();
    }

    [Fact]
    public async Task CreatePaymentIntent_DoesNotRetryOnNonTransientError()
    {
        var callCount = 0;
        _innerGateway.CreatePaymentIntentAsync(
            Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<Dictionary<string, string>>())
            .ThrowsAsyncForAnyArgs(callInfo =>
            {
                callCount++;
                return new StripeException(HttpStatusCode.BadRequest, null, "Invalid card");
            });

        var gateway = new ResilientPaymentGateway(_innerGateway, _logger);

        var act = () => gateway.CreatePaymentIntentAsync(
            100m, "usd", new Dictionary<string, string>());

        await act.Should().ThrowAsync<StripeException>();
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task CreateRefund_RetriesOnTransientStripeError()
    {
        var callCount = 0;
        _innerGateway.CreateRefundAsync(Arg.Any<string>(), Arg.Any<decimal>())
            .ReturnsForAnyArgs(callInfo =>
            {
                callCount++;
                if (callCount <= 1)
                    throw new StripeException(HttpStatusCode.TooManyRequests, null, "Rate limited");

                return new RefundResult
                {
                    RefundId = "re_test",
                    Status = "succeeded"
                };
            });

        var gateway = new ResilientPaymentGateway(_innerGateway, _logger);

        var result = await gateway.CreateRefundAsync("pi_test", 50m);

        result.RefundId.Should().Be("re_test");
        callCount.Should().Be(2);
    }
}
