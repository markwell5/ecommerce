using System;

namespace Return.Application.Policies
{
    public static class ReturnPolicy
    {
        public const int DefaultReturnWindowDays = 30;
        public const int AutoApproveWindowDays = 14;
        public const decimal DefaultRestockingFeePercent = 0;

        /// <summary>
        /// Determines if a return can still be filed based on delivery date.
        /// </summary>
        public static bool IsWithinReturnWindow(DateTime deliveredAt, int windowDays = DefaultReturnWindowDays)
        {
            return DateTime.UtcNow <= deliveredAt.AddDays(windowDays);
        }

        /// <summary>
        /// Determines if a return should be auto-approved based on reason and timing.
        /// Auto-approve: "changed_mind" within 14 days, "wrong_item" always, "defective" always.
        /// Manual review: "other", or "changed_mind" after 14 days.
        /// </summary>
        public static bool ShouldAutoApprove(string reason, DateTime deliveredAt)
        {
            return reason switch
            {
                "defective" => true,
                "wrong_item" => true,
                "changed_mind" => DateTime.UtcNow <= deliveredAt.AddDays(AutoApproveWindowDays),
                _ => false
            };
        }

        /// <summary>
        /// Calculate restocking fee. Changed-mind returns after 14 days get 15% fee.
        /// </summary>
        public static decimal CalculateRestockingFee(string reason, DateTime deliveredAt, decimal itemAmount)
        {
            if (reason == "changed_mind" && DateTime.UtcNow > deliveredAt.AddDays(AutoApproveWindowDays))
                return Math.Round(itemAmount * 0.15m, 2);

            return 0;
        }
    }
}
