using AutoMapper;
using FluentAssertions;

namespace User.Application.Tests
{
    public class MapperProfileTests
    {
        [Fact]
        public void AutoMapper_Configuration_ShouldBeValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
            var act = () => config.AssertConfigurationIsValid();
            act.Should().NotThrow();
        }
    }
}
