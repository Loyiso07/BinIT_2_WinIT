using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class Resident
    {
        [Key]
        public int ResidentId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        public string Address { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }

        public int PointsBalance { get; set; } = 0;
        public int InfluencerPoints { get; set; } = 0;
        public double TotalCO2Saved { get; set; } = 0;
        public int TotalReferrals { get; set; } = 0;

        // ✅ FIX: Add MaxLength for index compatibility
        public string ReferralCode { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<RecyclingSubmission> Submissions { get; set; }
        public virtual ICollection<PointsTransaction> PointsTransactions { get; set; }
        public virtual ICollection<ReferralTransaction> ReferralsMade { get; set; }
    }
}