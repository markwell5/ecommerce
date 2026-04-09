using System;

namespace Ecommerce.Model.Loyalty.Response
{
    public class PointsTransactionResponse
    {
        public long Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Points { get; set; }
        public int BalanceAfter { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PointsHistoryResponse
    {
        public System.Collections.Generic.List<PointsTransactionResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
