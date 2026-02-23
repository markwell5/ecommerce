using AutoMapper;
using Cart.Application.Commands;
using Cart.Application.Interfaces;
using Cart.Application.Mappings;
using Cart.Application.Models;
using FluentAssertions;
using NSubstitute;

namespace Cart.Application.Tests.Commands;

public class RemoveFromCartTests
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;
    private readonly RemoveFromCartHandler _handler;

    public RemoveFromCartTests()
    {
        _repository = Substitute.For<ICartRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CartMappingProfile>());
        _mapper = config.CreateMapper();
        _handler = new RemoveFromCartHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_ExistingItem_RemovesFromCart()
    {
        var cart = new Models.Cart
        {
            Id = "cart-1",
            Items =
            [
                new CartItem { ProductId = 1, ProductName = "A", Quantity = 1, UnitPrice = 10m },
                new CartItem { ProductId = 2, ProductName = "B", Quantity = 1, UnitPrice = 20m }
            ]
        };
        _repository.GetCartAsync("cart-1").Returns(cart);

        var result = await _handler.Handle(
            new RemoveFromCartCommand("cart-1", 1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items[0].ProductId.Should().Be(2);
    }

    [Fact]
    public async Task Handle_CartNotFound_ReturnsNull()
    {
        _repository.GetCartAsync("missing").Returns((Models.Cart?)null);

        var result = await _handler.Handle(
            new RemoveFromCartCommand("missing", 1), CancellationToken.None);

        result.Should().BeNull();
    }
}
