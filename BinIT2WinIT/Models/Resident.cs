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

        // Address Fields
        public string Address { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }

        // ✅ ADD THESE FIELDS
        public string Province { get; set; }
        public string PostalCode { get; set; }

        // Points & Rewards
        public int PointsBalance { get; set; } = 0;
        public int InfluencerPoints { get; set; } = 0;
        public double TotalCO2Saved { get; set; } = 0;
        public int TotalReferrals { get; set; } = 0;

        // Referral
        [MaxLength(50)]
        [Index("IX_UniqueReferralCode", IsUnique = true)]
        public string ReferralCode { get; set; }

        // ✅ ADD THIS - Link to Community (Drop-Off Point)
        public int? DropOffPointId { get; set; }

        // Status
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("DropOffPointId")]
        public virtual DropOffPoint Community { get; set; }

        public virtual ICollection<RecyclingSubmission> Submissions { get; set; }
        public virtual ICollection<PointsTransaction> PointsTransactions { get; set; }
        public virtual ICollection<ReferralTransaction> ReferralsMade { get; set; }
    }
}