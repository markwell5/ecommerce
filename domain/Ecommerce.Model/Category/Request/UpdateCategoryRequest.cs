namespace Ecommerce.Model.Category.Request
{
    public class UpdateCategoryRequest
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public long? ParentId { get; set; }
    }
}
