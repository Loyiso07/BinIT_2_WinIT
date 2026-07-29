using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class RedemptionRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int ResidentId { get; set; }

        [Required]
        public int OptionId { get; set; }

        [Required]
        public int PointsUsed { get; set; }

        // ✅ FIXED: Remove Column TypeName or use default
        [Required]
        // [Column(TypeName = "decimal(10,2)")]  // ← REMOVE THIS LINE
        public decimal DiscountAmount { get; set; }

        [Required]
        [MaxLength(20)]
        public string UtilityType { get; set; }

        [Required]
        [MaxLength(20)]
        public string RequestStatus { get; set; } = "Pending";

        public DateTime RequestDate { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public DateTime? AppliedDate { get; set; }

        [MaxLength(50)]
        public string ReferenceNumber { get; set; }

        public string AdminNotes { get; set; }
        public string ApprovedBy { get; set; }

        [MaxLength(50)]
        public string UtilityAccountNumber { get; set; }

        // Navigation
        [ForeignKey("ResidentId")]
        public virtual Resident Resident { get; set; }

        [ForeignKey("OptionId")]
        public virtual RedemptionOption RedemptionOption { get; set; }
    }
}