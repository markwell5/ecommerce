using AutoMapper;
using Stock.Application;

namespace Stock.Application.Tests;

public class MapperProfileTests
{
    [Fact]
    public void AutoMapper_Configuration_ShouldBeValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
        config.AssertConfigurationIsValid();
    }
}
