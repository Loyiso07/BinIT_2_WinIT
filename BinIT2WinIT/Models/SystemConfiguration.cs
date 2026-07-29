using System;
using System.ComponentModel.DataAnnotations;

namespace BinIT2WinIT.Models
{
    public class SystemConfiguration
    {
        [Key]
        public int ConfigId { get; set; }

        [Required]
        public string ConfigKey { get; set; }

        [Required]
        public string ConfigValue { get; set; }

        public string Description { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public string UpdatedBy { get; set; }

        // ✅ NEW: Redemption Configuration Properties
        public bool IsRedemptionEnabled { get; set; } = true;

        [Range(1, 10000)]
        public int MinRedeemablePoints { get; set; } = 100;

        [Range(1, 100000)]
        public int MaxRedeemablePoints { get; set; } = 5000;

        [Range(0.01, 100)]
        public decimal WaterDiscountRate { get; set; } = 0.50m; // 50% of points value

        [Range(0.01, 100)]
        public decimal ElectricityDiscountRate { get; set; } = 0.40m; // 40% of points value

        [Range(0.01, 100)]
        public decimal ComboDiscountRate { get; set; } = 0.30m; // 30% of points value

        public int RedemptionProcessingDays { get; set; } = 3; // Business days
        public string RedemptionTerms { get; set; } = "Redemption is subject to municipal approval. Discount will be applied to your next utility bill.";

        // ✅ NEW: Redemption Option Properties
        public int? DefaultOptionId { get; set; }
        public bool AutoApproveRedemption { get; set; } = false; // For testing/demo
    }
}