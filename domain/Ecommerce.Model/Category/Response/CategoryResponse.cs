using System.Collections.Generic;

namespace Ecommerce.Model.Category.Response
{
    public class CategoryResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public long? ParentId { get; set; }
        public List<CategoryResponse> Children { get; set; } = new();
    }
}
