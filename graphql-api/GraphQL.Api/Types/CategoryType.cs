namespace GraphQL.Api.Types;

public class Category
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public long? ParentId { get; set; }
    public List<Category> Children { get; set; } = new();
}
