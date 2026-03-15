using Ecommerce.Shared.Infrastructure.Validation;
using FluentAssertions;
using MediatR;

namespace Ecommerce.Shared.Infrastructure.Tests;

public class InputSanitizationBehaviorTests
{
    private readonly InputSanitizationBehavior<TestCommand, TestResponse> _behavior = new();

    [Fact]
    public async Task Handle_HtmlInStringProperty_EncodesIt()
    {
        var command = new TestCommand { Name = "<script>alert('xss')</script>" };

        await _behavior.Handle(command, () => Task.FromResult(new TestResponse()), CancellationToken.None);

        command.Name.Should().Be("&lt;script&gt;alert(&#39;xss&#39;)&lt;/script&gt;");
    }

    [Fact]
    public async Task Handle_PlainText_RemainsUnchanged()
    {
        var command = new TestCommand { Name = "Normal Product Name" };

        await _behavior.Handle(command, () => Task.FromResult(new TestResponse()), CancellationToken.None);

        command.Name.Should().Be("Normal Product Name");
    }

    [Fact]
    public async Task Handle_TrimsWhitespace()
    {
        var command = new TestCommand { Name = "  padded  " };

        await _behavior.Handle(command, () => Task.FromResult(new TestResponse()), CancellationToken.None);

        command.Name.Should().Be("padded");
    }

    [Fact]
    public async Task Handle_NullString_RemainsNull()
    {
        var command = new TestCommand { Name = null };

        await _behavior.Handle(command, () => Task.FromResult(new TestResponse()), CancellationToken.None);

        command.Name.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NestedObject_SanitizesNestedStrings()
    {
        var command = new TestCommandWithNested
        {
            Detail = new TestDetail { Description = "<b>bold</b>" }
        };

        var behavior = new InputSanitizationBehavior<TestCommandWithNested, TestResponse>();

        await behavior.Handle(command, () => Task.FromResult(new TestResponse()), CancellationToken.None);

        command.Detail.Description.Should().Be("&lt;b&gt;bold&lt;/b&gt;");
    }

    [Fact]
    public async Task Handle_AmpersandAndQuotes_AreEncoded()
    {
        var command = new TestCommand { Name = "A & B \"quoted\"" };

        await _behavior.Handle(command, () => Task.FromResult(new TestResponse()), CancellationToken.None);

        command.Name.Should().Be("A &amp; B &quot;quoted&quot;");
    }

    [Fact]
    public async Task Handle_CallsNext()
    {
        var command = new TestCommand { Name = "test" };
        var nextCalled = false;

        await _behavior.Handle(command, () =>
        {
            nextCalled = true;
            return Task.FromResult(new TestResponse());
        }, CancellationToken.None);

        nextCalled.Should().BeTrue();
    }

    public class TestCommand : IRequest<TestResponse>
    {
        public string Name { get; set; }
    }

    public class TestCommandWithNested : IRequest<TestResponse>
    {
        public TestDetail Detail { get; set; }
    }

    public class TestDetail
    {
        public string Description { get; set; }
    }

    public class TestResponse { }
}
