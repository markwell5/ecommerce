namespace Ecommerce.Model.Review.Request
{
    public class CreateReviewRequest
    {
        public long ProductId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
