using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using Payment.Application;

namespace Payment.Application.Tests;

public class MapperProfileTests
{
    [Fact]
    public void AutoMapper_Configuration_ShouldBeValid()
    {
        var expr = new MapperConfigurationExpression();
        expr.AddProfile<MapperProfile>();
        var config = new MapperConfiguration(expr, NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
    }
}
