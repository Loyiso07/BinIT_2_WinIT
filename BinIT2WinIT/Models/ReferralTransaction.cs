using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class ReferralTransaction
    {
        [Key]
        public int ReferralId { get; set; }

        [Required]
        public int ReferrerId { get; set; }  // The person who referred

        [Required]
        public int NewResidentId { get; set; }  // The person who joined

        public string PromoCodeUsed { get; set; }

        [Required]
        public int InfluencerPointsEarned { get; set; }

        [Required]
        public int WelcomeBonusAwarded { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Completed";  // Completed, Pending, Reversed

        // Navigation Properties
        [ForeignKey("ReferrerId")]
        public virtual Resident Referrer { get; set; }

        [ForeignKey("NewResidentId")]
        public virtual Resident NewResident { get; set; }
    }
}