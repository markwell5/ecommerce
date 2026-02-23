using AutoMapper;
using Ecommerce.Model.Product.Response;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Product.Application;
using Product.Application.Queries;

namespace Product.Application.Tests.Queries;

public class SearchProductsQueryTests
{
    private readonly ProductDbContext _dbContext;
    private readonly IMapper _mapper;

    public SearchProductsQueryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ProductDbContext(options);

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MapperProfile>());
        _mapper = config.CreateMapper();

        SeedProducts();
    }

    private void SeedProducts()
    {
        _dbContext.Products.AddRange(
            new Entities.Product { Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Category = "Electronics", Price = 29.99m },
            new Entities.Product { Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard", Category = "Electronics", Price = 89.99m },
            new Entities.Product { Name = "Running Shoes", Description = "Lightweight running shoes", Category = "Sports", Price = 119.99m },
            new Entities.Product { Name = "Yoga Mat", Description = "Non-slip yoga mat", Category = "Sports", Price = 24.99m },
            new Entities.Product { Name = "Coffee Beans", Description = "Premium arabica coffee beans", Category = "Food", Price = 14.99m }
        );
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnMatchingProducts()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { Category = "Electronics" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.Category.Should().Be("Electronics"));
    }

    [Fact]
    public async Task Handle_WithMinPriceFilter_ShouldReturnProductsAboveMinPrice()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { MinPrice = 50m };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.Price.Should().BeGreaterOrEqualTo(50m));
    }

    [Fact]
    public async Task Handle_WithMaxPriceFilter_ShouldReturnProductsBelowMaxPrice()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { MaxPrice = 25m };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.Price.Should().BeLessOrEqualTo(25m));
    }

    [Fact]
    public async Task Handle_WithPriceRange_ShouldReturnProductsInRange()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { MinPrice = 20m, MaxPrice = 100m };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.Items.Should().AllSatisfy(p =>
        {
            p.Price.Should().BeGreaterOrEqualTo(20m);
            p.Price.Should().BeLessOrEqualTo(100m);
        });
    }

    [Fact]
    public async Task Handle_WithCategoryAndPriceRange_ShouldCombineFilters()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { Category = "Sports", MaxPrice = 50m };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Yoga Mat");
    }

    [Fact]
    public async Task Handle_SortByPriceAsc_ShouldReturnSortedResults()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { SortBy = "price", SortDirection = "asc" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Select(p => p.Price).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_SortByPriceDesc_ShouldReturnSortedResults()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { SortBy = "price", SortDirection = "desc" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Select(p => p.Price).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_SortByName_ShouldReturnSortedResults()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { SortBy = "name" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Select(p => p.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { Page = 2, PageSize = 2 };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task Handle_NoFilters_ShouldReturnAllProducts()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ShouldReturnEmpty()
    {
        var handler = new SearchProductsQueryHandler(_dbContext, _mapper);
        var query = new SearchProductsQuery { Category = "NonExistent" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
