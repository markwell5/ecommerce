namespace Core.Domain.Commands
{
    public class CreateProduct : BaseEvent
    {
        public CreateProduct(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}
