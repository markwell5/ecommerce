using Cart.Application.Commands;
using Cart.Application.Interfaces;
using Cart.Application.Models;
using FluentAssertions;
using NSubstitute;

namespace Cart.Application.Tests.Commands;

public class ClearCartTests
{
    private readonly ICartRepository _repository;
    private readonly ClearCartHandler _handler;

    public ClearCartTests()
    {
        _repository = Substitute.For<ICartRepository>();
        _handler = new ClearCartHandler(_repository);
    }

    [Fact]
    public async Task Handle_ExistingCart_DeletesAndReturnsTrue()
    {
        var cart = new Models.Cart { Id = "cart-1", Items = [new CartItem { ProductId = 1 }] };
        _repository.GetCartAsync("cart-1").Returns(cart);

        var result = await _handler.Handle(new ClearCartCommand("cart-1"), CancellationToken.None);

        result.Should().BeTrue();
        await _repository.Received(1).DeleteCartAsync("cart-1");
    }

    [Fact]
    public async Task Handle_CartNotFound_ReturnsFalse()
    {
        _repository.GetCartAsync("missing").Returns((Models.Cart?)null);

        var result = await _handler.Handle(new ClearCartCommand("missing"), CancellationToken.None);

        result.Should().BeFalse();
    }
}
