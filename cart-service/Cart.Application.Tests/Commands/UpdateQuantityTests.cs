using AutoMapper;
using Cart.Application.Commands;
using Cart.Application.Interfaces;
using Cart.Application.Mappings;
using Cart.Application.Models;
using FluentAssertions;
using NSubstitute;

namespace Cart.Application.Tests.Commands;

public class UpdateQuantityTests
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;
    private readonly UpdateQuantityHandler _handler;

    public UpdateQuantityTests()
    {
        _repository = Substitute.For<ICartRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CartMappingProfile>());
        _mapper = config.CreateMapper();
        _handler = new UpdateQuantityHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_ExistingItem_UpdatesQuantity()
    {
        var cart = new Models.Cart
        {
            Id = "cart-1",
            Items = [new CartItem { ProductId = 1, ProductName = "A", Quantity = 2, UnitPrice = 10m }]
        };
        _repository.GetCartAsync("cart-1").Returns(cart);

        var result = await _handler.Handle(
            new UpdateQuantityCommand("cart-1", 1, 5), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ItemNotInCart_ReturnsNull()
    {
        var cart = new Models.Cart { Id = "cart-1", Items = [] };
        _repository.GetCartAsync("cart-1").Returns(cart);

        var result = await _handler.Handle(
            new UpdateQuantityCommand("cart-1", 99, 5), CancellationToken.None);

        result.Should().BeNull();
    }
}
