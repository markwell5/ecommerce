namespace Ecommerce.Events.Product
{
    public class ProductDeleted : EventBase
    {
        public long Key { get; init; }
    }
}
