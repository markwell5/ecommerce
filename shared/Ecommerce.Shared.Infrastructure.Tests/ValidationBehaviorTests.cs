using Ecommerce.Shared.Infrastructure.Validation;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

namespace Ecommerce.Shared.Infrastructure.Tests;

public class ValidationBehaviorTests
{
    private record TestRequest(string Name) : IRequest<string>;

    private class TestValidator : AbstractValidator<TestRequest>
    {
        public TestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCallNext()
    {
        var validators = new[] { new TestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("ok");

        var result = await behavior.Handle(new TestRequest("valid"), next, CancellationToken.None);

        result.Should().Be("ok");
        await next.Received(1)();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ShouldThrowValidationException()
    {
        var validators = new[] { new TestValidator() };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var next = Substitute.For<RequestHandlerDelegate<string>>();

        var act = () => behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<Validation.ValidationException>();
        await next.DidNotReceive()();
    }

    [Fact]
    public async Task Handle_NoValidators_ShouldCallNext()
    {
        var validators = Array.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next().Returns("ok");

        var result = await behavior.Handle(new TestRequest(""), next, CancellationToken.None);

        result.Should().Be("ok");
    }
}
