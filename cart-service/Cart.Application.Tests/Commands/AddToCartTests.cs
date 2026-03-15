using AutoMapper;
using Cart.Application.Commands;
using Cart.Application.Interfaces;
using Cart.Application.Mappings;
using Cart.Application.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Cart.Application.Tests.Commands;

public class AddToCartTests
{
    private readonly ICartRepository _repository;
    private readonly IProductCatalogClient _productCatalog;
    private readonly IMapper _mapper;
    private readonly ILogger<AddToCartHandler> _logger;
    private readonly AddToCartHandler _handler;

    public AddToCartTests()
    {
        _repository = Substitute.For<ICartRepository>();
        _productCatalog = Substitute.For<IProductCatalogClient>();
        _logger = Substitute.For<ILogger<AddToCartHandler>>();

        var expr = new MapperConfigurationExpression();
        expr.AddProfile<CartMappingProfile>();
        var config = new MapperConfiguration(expr, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();

        _handler = new AddToCartHandler(_repository, _productCatalog, _mapper, _logger);
    }

    [Fact]
    public async Task Handle_NewCart_CreatesCartWithItem()
    {
        _repository.GetCartAsync("cart-1").Returns((Models.Cart?)null);
        _productCatalog.GetProductAsync(1, Arg.Any<CancellationToken>())
            .Returns(new ProductInfo(1, "Test Product", 9.99m));

        var result = await _handler.Handle(
            new AddToCartCommand("cart-1", 1, 2), CancellationToken.None);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductId.Should().Be(1);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(9.99m);

        await _repository.Received(1).SaveCartAsync(Arg.Is<Models.Cart>(c => c.Id == "cart-1"));
    }

    [Fact]
    public async Task Handle_ExistingItem_IncrementsQuantity()
    {
        var existingCart = new Models.Cart
        {
            Id = "cart-1",
            Items = [new CartItem { ProductId = 1, ProductName = "Test", Quantity = 2, UnitPrice = 9.99m }]
        };
        _repository.GetCartAsync("cart-1").Returns(existingCart);
        _productCatalog.GetProductAsync(1, Arg.Any<CancellationToken>())
            .Returns(new ProductInfo(1, "Test", 9.99m));

        var result = await _handler.Handle(
            new AddToCartCommand("cart-1", 1, 3), CancellationToken.None);

        result.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsKeyNotFoundException()
    {
        _productCatalog.GetProductAsync(999, Arg.Any<CancellationToken>())
            .Returns((ProductInfo?)null);

        var act = () => _handler.Handle(
            new AddToCartCommand("cart-1", 999, 1), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
