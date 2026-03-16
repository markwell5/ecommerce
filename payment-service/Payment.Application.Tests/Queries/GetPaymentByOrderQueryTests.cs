using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Application;
using Payment.Application.Queries;

namespace Payment.Application.Tests.Queries;

public class GetPaymentByOrderQueryTests
{
    private readonly PaymentDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetPaymentByOrderQueryTests()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PaymentDbContext(options);

        var expr = new MapperConfigurationExpression();
        expr.AddProfile<MapperProfile>();
        var config = new MapperConfiguration(expr, NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task Handle_ExistingOrder_ShouldReturnPayment()
    {
        var orderId = Guid.NewGuid();
        _dbContext.Payments.Add(new Entities.Payment
        {
            OrderId = orderId,
            CustomerId = "cust-1",
            Amount = 75.50m,
            Currency = "usd",
            Status = "Succeeded",
            StripePaymentIntentId = "pi_test"
        });
        await _dbContext.SaveChangesAsync();

        var handler = new GetPaymentByOrderQueryHandler(_dbContext, _mapper);
        var result = await handler.Handle(new GetPaymentByOrderQuery(orderId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.OrderId.Should().Be(orderId);
        result.Amount.Should().Be(75.50m);
        result.Status.Should().Be("Succeeded");
    }

    [Fact]
    public async Task Handle_NonExistingOrder_ShouldReturnNull()
    {
        var handler = new GetPaymentByOrderQueryHandler(_dbContext, _mapper);
        var result = await handler.Handle(new GetPaymentByOrderQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
