namespace GraphQL.Api.Types;

public class WishlistItem2
{
    public long Id { get; set; }
    public string CustomerId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsDefault { get; set; }
    public string ShareToken { get; set; } = default!;
    public bool IsPublic { get; set; }
    public List<WishlistItemEntry> Items { get; set; } = [];
    public string CreatedAt { get; set; } = default!;
    public string UpdatedAt { get; set; } = default!;
}

public class WishlistItemEntry
{
    public long Id { get; set; }
    public long WishlistId { get; set; }
    public long ProductId { get; set; }
    public bool NotifyOnRestock { get; set; }
    public string AddedAt { get; set; } = default!;
}
