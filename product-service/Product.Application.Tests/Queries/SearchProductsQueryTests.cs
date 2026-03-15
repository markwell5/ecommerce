using System.Collections.Generic;
using Ecommerce.Model.Product.Response;
using FluentAssertions;
using NSubstitute;
using Product.Application.Queries;
using Product.Application.Search;

namespace Product.Application.Tests.Queries;

public class SearchProductsQueryTests
{
    private readonly IProductSearchService _searchService;

    private static readonly List<ProductSearchDocument> AllProducts =
    [
        new() { Id = 1, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", Category = "Electronics", Price = 29.99m },
        new() { Id = 2, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard", Category = "Electronics", Price = 89.99m },
        new() { Id = 3, Name = "Running Shoes", Description = "Lightweight running shoes", Category = "Sports", Price = 119.99m },
        new() { Id = 4, Name = "Yoga Mat", Description = "Non-slip yoga mat", Category = "Sports", Price = 24.99m },
        new() { Id = 5, Name = "Coffee Beans", Description = "Premium arabica coffee beans", Category = "Food", Price = 14.99m }
    ];

    public SearchProductsQueryTests()
    {
        _searchService = Substitute.For<IProductSearchService>();
    }

    private void SetupSearchResult(List<ProductSearchDocument> items, long? totalCount = null)
    {
        _searchService.SearchAsync(Arg.Any<SearchProductsQuery>(), Arg.Any<CancellationToken>())
            .Returns(new ProductSearchResult
            {
                Items = items,
                TotalCount = totalCount ?? items.Count,
                Facets = new SearchFacets
                {
                    Categories = new Dictionary<string, long>
                    {
                        { "Electronics", 2 }, { "Sports", 2 }, { "Food", 1 }
                    },
                    MinPrice = 14.99m,
                    MaxPrice = 119.99m
                }
            });
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldReturnMatchingProducts()
    {
        var electronics = AllProducts.Where(p => p.Category == "Electronics").ToList();
        SetupSearchResult(electronics);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { Category = "Electronics" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.Category.Should().Be("Electronics"));
    }

    [Fact]
    public async Task Handle_WithMinPriceFilter_ShouldReturnProductsAboveMinPrice()
    {
        var expensive = AllProducts.Where(p => p.Price >= 50m).ToList();
        SetupSearchResult(expensive);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { MinPrice = 50m };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.Price.Should().BeGreaterOrEqualTo(50m));
    }

    [Fact]
    public async Task Handle_WithMaxPriceFilter_ShouldReturnProductsBelowMaxPrice()
    {
        var cheap = AllProducts.Where(p => p.Price <= 25m).ToList();
        SetupSearchResult(cheap);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { MaxPrice = 25m };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(p => p.Price.Should().BeLessOrEqualTo(25m));
    }

    [Fact]
    public async Task Handle_WithPriceRange_ShouldReturnProductsInRange()
    {
        var inRange = AllProducts.Where(p => p.Price >= 20m && p.Price <= 100m).ToList();
        SetupSearchResult(inRange);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { MinPrice = 20m, MaxPrice = 100m };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithCategoryAndPriceRange_ShouldCombineFilters()
    {
        var yogaMat = AllProducts.Where(p => p.Category == "Sports" && p.Price <= 50m).ToList();
        SetupSearchResult(yogaMat);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { Category = "Sports", MaxPrice = 50m };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Yoga Mat");
    }

    [Fact]
    public async Task Handle_SortByPriceAsc_ShouldReturnSortedResults()
    {
        var sorted = AllProducts.OrderBy(p => p.Price).ToList();
        SetupSearchResult(sorted);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { SortBy = "price", SortDirection = "asc" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Select(p => p.Price).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_SortByPriceDesc_ShouldReturnSortedResults()
    {
        var sorted = AllProducts.OrderByDescending(p => p.Price).ToList();
        SetupSearchResult(sorted);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { SortBy = "price", SortDirection = "desc" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Select(p => p.Price).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Handle_SortByName_ShouldReturnSortedResults()
    {
        var sorted = AllProducts.OrderBy(p => p.Name).ToList();
        SetupSearchResult(sorted);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { SortBy = "name" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Select(p => p.Name).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        var page2 = AllProducts.Skip(2).Take(2).ToList();
        SetupSearchResult(page2, totalCount: 5);
        var handler = new SearchProductsQueryHandler(_searchService);
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
        SetupSearchResult(AllProducts);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_NonExistentCategory_ShouldReturnEmpty()
    {
        SetupSearchResult([]);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery { Category = "NonExistent" };

        var result = await handler.Handle(query, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnFacets()
    {
        SetupSearchResult(AllProducts);
        var handler = new SearchProductsQueryHandler(_searchService);
        var query = new SearchProductsQuery();

        var result = await handler.Handle(query, CancellationToken.None);

        result.CategoryFacets.Should().ContainKey("Electronics");
        result.CategoryFacets["Electronics"].Should().Be(2);
        result.PriceRangeMin.Should().Be(14.99m);
        result.PriceRangeMax.Should().Be(119.99m);
    }
}
