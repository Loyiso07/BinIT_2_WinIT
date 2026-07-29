using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class RedemptionOption
    {
        [Key]
        public int OptionId { get; set; }

        [Required]
        [MaxLength(20)]
        public string UtilityType { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }

        [Required]
        public int PointsRequired { get; set; }

        // ✅ FIXED: Remove Column TypeName or use default
        [Required]
        // [Column(TypeName = "decimal(10,2)")]  // ← REMOVE THIS LINE
        public decimal DiscountAmount { get; set; }

        public string Icon { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ExpiryDate { get; set; }

        public virtual ICollection<RedemptionRequest> RedemptionRequests { get; set; }
    }
}