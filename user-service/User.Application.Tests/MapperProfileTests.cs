using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace User.Application.Tests
{
    public class MapperProfileTests
    {
        [Fact]
        public void AutoMapper_Configuration_ShouldBeValid()
        {
            var expr = new MapperConfigurationExpression();
            expr.AddProfile<MapperProfile>();
            var config = new MapperConfiguration(expr, NullLoggerFactory.Instance);
            var act = () => config.AssertConfigurationIsValid();
            act.Should().NotThrow();
        }
    }
}
