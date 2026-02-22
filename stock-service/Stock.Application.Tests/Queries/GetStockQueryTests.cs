using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Stock.Application;
using Stock.Application.Entities;
using Stock.Application.Queries;

namespace Stock.Application.Tests.Queries;

public class GetStockQueryTests
{
    private readonly StockDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetStockQueryTests()
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new StockDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldReturnStockResponse()
    {
        _dbContext.StockItems.Add(new StockItem
        {
            ProductId = 42,
            AvailableQuantity = 75,
            ReservedQuantity = 10
        });
        await _dbContext.SaveChangesAsync();

        var handler = new GetStockQueryHandler(_dbContext, _mapper);
        var result = await handler.Handle(new GetStockQuery(42), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProductId.Should().Be(42);
        result.AvailableQuantity.Should().Be(75);
        result.ReservedQuantity.Should().Be(10);
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnNull()
    {
        var handler = new GetStockQueryHandler(_dbContext, _mapper);
        var result = await handler.Handle(new GetStockQuery(999), CancellationToken.None);

        result.Should().BeNull();
    }
}
