using System.Collections.Generic;

namespace Ecommerce.Model.Product.Response;

public class ProductSearchResponse : PagedResponse<ProductResponse>
{
    public Dictionary<string, long> CategoryFacets { get; set; } = new();
    public decimal? PriceRangeMin { get; set; }
    public decimal? PriceRangeMax { get; set; }
}
