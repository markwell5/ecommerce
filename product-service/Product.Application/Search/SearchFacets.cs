using System.Collections.Generic;

namespace Product.Application.Search;

public class SearchFacets
{
    public Dictionary<string, long> Categories { get; set; } = new();
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}
