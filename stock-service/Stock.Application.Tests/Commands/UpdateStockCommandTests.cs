using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Stock.Application;
using Stock.Application.Commands;
using Stock.Application.Entities;

namespace Stock.Application.Tests.Commands;

public class UpdateStockCommandTests
{
    private readonly StockDbContext _dbContext;
    private readonly IMapper _mapper;

    public UpdateStockCommandTests()
    {
        var options = new DbContextOptionsBuilder<StockDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new StockDbContext(options);

        var expr = new MapperConfigurationExpression();
        expr.AddProfile<MapperProfile>();
        var config = new MapperConfiguration(expr, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingProduct_ShouldUpdateQuantity()
    {
        _dbContext.StockItems.Add(new StockItem
        {
            ProductId = 1,
            AvailableQuantity = 50,
            ReservedQuantity = 0
        });
        await _dbContext.SaveChangesAsync();

        var handler = new UpdateStockCommandHandler(_dbContext, _mapper);
        var result = await handler.Handle(new UpdateStockCommand(1, 200), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AvailableQuantity.Should().Be(200);
        result.ProductId.Should().Be(1);
    }

    [Fact]
    public async Task Handle_NonExistingProduct_ShouldReturnNull()
    {
        var handler = new UpdateStockCommandHandler(_dbContext, _mapper);
        var result = await handler.Handle(new UpdateStockCommand(999, 100), CancellationToken.None);

        result.Should().BeNull();
    }
}
