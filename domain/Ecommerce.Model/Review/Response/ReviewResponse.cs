using System;

namespace Ecommerce.Model.Review.Response
{
    public class ReviewResponse
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string CustomerId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductRatingResponse
    {
        public long ProductId { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
