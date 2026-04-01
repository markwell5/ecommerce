namespace Ecommerce.Events.Review
{
    public class ReviewCreated : EventBase
    {
        public long Id { get; init; }
        public long ProductId { get; init; }
        public string CustomerId { get; init; }
        public int Rating { get; init; }
    }
}
