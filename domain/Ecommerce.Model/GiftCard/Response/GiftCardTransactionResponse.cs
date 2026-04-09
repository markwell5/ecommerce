using System;

namespace Ecommerce.Model.GiftCard.Response
{
    public class GiftCardTransactionResponse
    {
        public long Id { get; set; }
        public long GiftCardId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string? OrderId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class GiftCardTransactionHistoryResponse
    {
        public System.Collections.Generic.List<GiftCardTransactionResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
