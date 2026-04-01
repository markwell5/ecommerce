namespace GraphQL.Api.Types;

public class StockLevel
{
    public long ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
}
