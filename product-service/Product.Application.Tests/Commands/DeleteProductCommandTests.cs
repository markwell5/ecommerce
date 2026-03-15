using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Product.Application;
using Product.Application.Caching;
using Product.Application.Commands;

namespace Product.Application.Tests.Commands;

public class DeleteProductCommandTests
{
    private readonly ProductDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IProductCacheInvalidator _cacheInvalidator;

    public DeleteProductCommandTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ProductDbContext(options);
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _cacheInvalidator = Substitute.For<IProductCacheInvalidator>();
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldReturnTrue()
    {
        _dbContext.Products.Add(new Product.Application.Entities.Product
        {
            Name = "To Delete",
            Description = "Test",
            Price = 1.00m
        });
        await _dbContext.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(_dbContext, _publishEndpoint, _cacheInvalidator);
        var result = await handler.Handle(new DeleteProductCommand(1), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnFalse()
    {
        var handler = new DeleteProductCommandHandler(_dbContext, _publishEndpoint, _cacheInvalidator);
        var result = await handler.Handle(new DeleteProductCommand(999), CancellationToken.None);

        result.Should().BeFalse();
    }
}
