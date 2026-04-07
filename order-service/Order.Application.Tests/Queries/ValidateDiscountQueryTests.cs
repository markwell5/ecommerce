using Ecommerce.Model.Discount.Response;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Order.Application.Entities;
using Order.Application.Queries;

namespace Order.Application.Tests.Queries;

public class ValidateDiscountQueryTests
{
    private static OrderDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new OrderDbContext(options);
    }

    private static Coupon CreateValidCoupon(string discountType = "percentage", decimal value = 10m) => new()
    {
        Code = "SAVE10",
        DiscountType = discountType,
        Value = value,
        MinOrderAmount = 20m,
        MaxUses = 100,
        CurrentUses = 0,
        ExpiresAt = DateTime.UtcNow.AddDays(30),
        IsActive = true
    };

    [Fact]
    public async Task Handle_ValidPercentageCoupon_ReturnsCorrectDiscount()
    {
        var db = CreateInMemoryDb();
        db.Coupons.Add(CreateValidCoupon("percentage", 15m));
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 100m), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(15m); // 15% of 100
        result.DiscountType.Should().Be("percentage");
        result.CouponCode.Should().Be("SAVE10");
    }

    [Fact]
    public async Task Handle_ValidFixedCoupon_ReturnsCorrectDiscount()
    {
        var db = CreateInMemoryDb();
        db.Coupons.Add(CreateValidCoupon("fixed", 5m));
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 100m), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(5m);
        result.DiscountType.Should().Be("fixed");
    }

    [Fact]
    public async Task Handle_FixedCoupon_CapsAtOrderAmount()
    {
        var db = CreateInMemoryDb();
        var coupon = CreateValidCoupon("fixed", 50m);
        coupon.MinOrderAmount = 0;
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 30m), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(30m); // capped at order amount
    }

    [Fact]
    public async Task Handle_ValidFreeShippingCoupon_ReturnsZeroDiscountAmount()
    {
        var db = CreateInMemoryDb();
        db.Coupons.Add(CreateValidCoupon("freeshipping", 0m));
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 100m), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(0m);
        result.DiscountType.Should().Be("freeshipping");
    }

    [Fact]
    public async Task Handle_ExpiredCoupon_ReturnsInvalid()
    {
        var db = CreateInMemoryDb();
        var coupon = CreateValidCoupon();
        coupon.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 100m), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Coupon has expired");
    }

    [Fact]
    public async Task Handle_InactiveCoupon_ReturnsInvalid()
    {
        var db = CreateInMemoryDb();
        var coupon = CreateValidCoupon();
        coupon.IsActive = false;
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 100m), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Coupon is inactive");
    }

    [Fact]
    public async Task Handle_MaxUsesReached_ReturnsInvalid()
    {
        var db = CreateInMemoryDb();
        var coupon = CreateValidCoupon();
        coupon.MaxUses = 5;
        coupon.CurrentUses = 5;
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 100m), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Coupon has reached maximum uses");
    }

    [Fact]
    public async Task Handle_UnlimitedUses_ReturnsValid()
    {
        var db = CreateInMemoryDb();
        var coupon = CreateValidCoupon();
        coupon.MaxUses = 0; // unlimited
        coupon.CurrentUses = 9999;
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 100m), CancellationToken.None);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_OrderBelowMinimum_ReturnsInvalid()
    {
        var db = CreateInMemoryDb();
        db.Coupons.Add(CreateValidCoupon()); // MinOrderAmount = 20
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 10m), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Minimum order amount is 20");
    }

    [Fact]
    public async Task Handle_CouponNotFound_ReturnsInvalid()
    {
        var db = CreateInMemoryDb();
        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("NONEXISTENT", 100m), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Coupon not found");
    }

    [Fact]
    public async Task Handle_EmptyCouponCode_ReturnsInvalid()
    {
        var db = CreateInMemoryDb();
        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("", 100m), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Coupon code is required");
    }

    [Fact]
    public async Task Handle_NullCouponCode_ReturnsInvalid()
    {
        var db = CreateInMemoryDb();
        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery(null, 100m), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Coupon code is required");
    }

    [Fact]
    public async Task Handle_CaseInsensitiveLookup_FindsCoupon()
    {
        var db = CreateInMemoryDb();
        db.Coupons.Add(CreateValidCoupon());
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("save10", 100m), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.CouponCode.Should().Be("SAVE10");
    }

    [Fact]
    public async Task Handle_PercentageDiscount_RoundsToTwoDecimalPlaces()
    {
        var db = CreateInMemoryDb();
        var coupon = CreateValidCoupon("percentage", 33.33m);
        db.Coupons.Add(coupon);
        await db.SaveChangesAsync();

        var handler = new ValidateDiscountQueryHandler(db);
        var result = await handler.Handle(new ValidateDiscountQuery("SAVE10", 100m), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.DiscountAmount.Should().Be(33.33m);
    }
}
