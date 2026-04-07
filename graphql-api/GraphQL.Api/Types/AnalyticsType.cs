namespace GraphQL.Api.Types;

public class SalesOverview
{
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AvgOrderValue { get; set; }
    public int CancelledCount { get; set; }
    public int ReturnedCount { get; set; }
    public int NewCustomerCount { get; set; }
}

public class StatusBreakdownItem
{
    public string Status { get; set; } = default!;
    public int Count { get; set; }
}

public class DailyRevenuePoint
{
    public string Date { get; set; } = default!;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
