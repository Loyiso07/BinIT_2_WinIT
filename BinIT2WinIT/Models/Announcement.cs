using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BinIT2WinIT.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }

        [Display(Name = "Reward Type")]
        public string RewardType { get; set; } // Voucher, CommunityReward, General

        [Display(Name = "Target Audience")]
        public string TargetAudience { get; set; } // All, Residents, Officers, Admins

        [Display(Name = "Minimum Points Required")]
        public int? MinPointsRequired { get; set; }

        [Display(Name = "Voucher Code")]
        public string VoucherCode { get; set; }

        [Display(Name = "Community Reward")]
        public string CommunityReward { get; set; } // "Park Makeover", "New Playground", etc.

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; }
    }
}