namespace Core.Domain.Events
{
    public class ProductCreated : BaseEvent
    {
        public ProductCreated(int id)
        {
            Id = id;
        }

        public int Id { get; set; }
    }
}
