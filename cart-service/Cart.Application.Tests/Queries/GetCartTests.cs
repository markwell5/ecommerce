using AutoMapper;
using Cart.Application.Interfaces;
using Cart.Application.Mappings;
using Cart.Application.Models;
using Cart.Application.Queries;
using FluentAssertions;
using NSubstitute;

namespace Cart.Application.Tests.Queries;

public class GetCartTests
{
    private readonly ICartRepository _repository;
    private readonly IMapper _mapper;
    private readonly GetCartHandler _handler;

    public GetCartTests()
    {
        _repository = Substitute.For<ICartRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CartMappingProfile>());
        _mapper = config.CreateMapper();
        _handler = new GetCartHandler(_repository, _mapper);
    }

    [Fact]
    public async Task Handle_ExistingCart_ReturnsMappedDto()
    {
        var cart = new Models.Cart
        {
            Id = "cart-1",
            Items = [new CartItem { ProductId = 1, ProductName = "A", Quantity = 3, UnitPrice = 10m }]
        };
        _repository.GetCartAsync("cart-1").Returns(cart);

        var result = await _handler.Handle(new GetCartQuery("cart-1"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be("cart-1");
        result.Items.Should().HaveCount(1);
        result.Items[0].LineTotal.Should().Be(30m);
        result.TotalPrice.Should().Be(30m);
    }

    [Fact]
    public async Task Handle_CartNotFound_ReturnsNull()
    {
        _repository.GetCartAsync("missing").Returns((Models.Cart?)null);

        var result = await _handler.Handle(new GetCartQuery("missing"), CancellationToken.None);

        result.Should().BeNull();
    }
}
