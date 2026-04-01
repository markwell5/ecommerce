namespace GraphQL.Api.Types;

public class Review
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string CustomerId { get; set; } = default!;
    public int Rating { get; set; }
    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;
    public string CreatedAt { get; set; } = default!;
}

public class ProductRating
{
    public long ProductId { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
}

public class ReviewConnection
{
    public List<Review> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
