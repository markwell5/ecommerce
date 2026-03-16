using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Product.Application;
using Product.Application.Queries;

namespace Product.Application.Tests.Queries;

public class GetProductQueryTests
{
    private readonly ProductDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetProductQueryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ProductDbContext(options);

        var expr = new MapperConfigurationExpression();
        expr.AddProfile<MapperProfile>();
        var config = new MapperConfiguration(expr, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldReturnResponse()
    {
        _dbContext.Products.Add(new Product.Application.Entities.Product
        {
            Name = "Found",
            Description = "Desc",
            Price = 3.50m
        });
        await _dbContext.SaveChangesAsync();

        var handler = new GetProductQueryHandler(_dbContext, _mapper);
        var result = await handler.Handle(new GetProductQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Found");
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnNull()
    {
        var handler = new GetProductQueryHandler(_dbContext, _mapper);
        var result = await handler.Handle(new GetProductQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }
}
