using AutoMapper;
using Ecommerce.Model.Product.Request;
using Ecommerce.Model.Product.Response;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Product.Application;
using Product.Application.Commands;

namespace Product.Application.Tests.Commands;

public class CreateProductCommandTests
{
    private readonly ProductDbContext _dbContext;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateProductCommandTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ProductDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
        _mapper = config.CreateMapper();

        _publishEndpoint = Substitute.For<IPublishEndpoint>();
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_AndReturnResponse()
    {
        var handler = new CreateProductCommandHandler(_dbContext, _mapper, _publishEndpoint);
        var command = new CreateProductCommand(new CreateProductRequest
        {
            Name = "Test Product",
            Description = "A test product",
            Price = 9.99m
        });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Product");
        result.Description.Should().Be("A test product");
        result.Price.Should().Be(9.99m);
        result.Id.Should().BeGreaterThan(0);

        var saved = await _dbContext.Products.FindAsync(result.Id);
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldPublishProductCreatedEvent()
    {
        var handler = new CreateProductCommandHandler(_dbContext, _mapper, _publishEndpoint);
        var command = new CreateProductCommand(new CreateProductRequest
        {
            Name = "Event Product",
            Description = "Test",
            Price = 5.00m
        });

        await handler.Handle(command, CancellationToken.None);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<Ecommerce.Events.Product.ProductCreated>(e => e.Id > 0),
            Arg.Any<CancellationToken>());
    }
}
