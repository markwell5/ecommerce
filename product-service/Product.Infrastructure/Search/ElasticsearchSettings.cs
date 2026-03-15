namespace Product.Infrastructure.Search;

public class ElasticsearchSettings
{
    public string Url { get; set; } = "http://localhost:9200";
    public string IndexName { get; set; } = "products";
}
