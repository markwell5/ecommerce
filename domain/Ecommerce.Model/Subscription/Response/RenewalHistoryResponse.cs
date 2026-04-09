using System;
using System.Collections.Generic;

namespace Ecommerce.Model.Subscription.Response
{
    public class RenewalHistoryResponse
    {
        public long Id { get; set; }
        public long SubscriptionId { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string FailureReason { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class RenewalHistoryPagedResponse
    {
        public List<RenewalHistoryResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
