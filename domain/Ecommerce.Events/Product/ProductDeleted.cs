namespace Ecommerce.Events.Product
{
    public class ProductDeleted : EventBase
    {
        public long Id { get; init; }
    }
}
