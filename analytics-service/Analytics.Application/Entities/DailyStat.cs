using System;

namespace Analytics.Application.Entities
{
    public class DailyStat
    {
        public long Id { get; set; }
        public DateOnly Date { get; set; }
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AvgOrderValue { get; set; }
        public int CancelledCount { get; set; }
        public int ReturnedCount { get; set; }
        public int NewCustomerCount { get; set; }
        public DateTime ComputedAt { get; set; }
    }
}
