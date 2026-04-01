using System;

namespace Product.Application.Entities;

public class Review
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Product Product { get; set; } = default!;
}
