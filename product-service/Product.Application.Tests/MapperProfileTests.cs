using AutoMapper;
using Product.Application;

namespace Product.Application.Tests;

public class MapperProfileTests
{
    [Fact]
    public void AutoMapper_Configuration_ShouldBeValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
        config.AssertConfigurationIsValid();
    }
}
