using Ecommerce.Events;
using Ecommerce.Events.Product;
using FluentAssertions;

namespace Ecommerce.Shared.Infrastructure.Tests;

public class EventBaseTests
{
    [Fact]
    public void EventName_ShouldBeTypeName()
    {
        var evt = new ProductCreated { Id = 1 };
        evt.EventName.Should().Be("ProductCreated");
    }

    [Fact]
    public void IdempotencyKey_ShouldBeUnique()
    {
        var evt1 = new ProductCreated { Id = 1 };
        var evt2 = new ProductCreated { Id = 1 };

        evt1.IdempotencyKey.Should().NotBe(evt2.IdempotencyKey);
    }

    [Fact]
    public void DateEmitted_ShouldBeRecentUtc()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var evt = new ProductCreated { Id = 1 };
        var after = DateTime.UtcNow.AddSeconds(1);

        evt.DateEmitted.Should().BeAfter(before).And.BeBefore(after);
    }
}
