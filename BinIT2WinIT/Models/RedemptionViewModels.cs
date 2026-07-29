using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BinIT2WinIT.Models
{
    public class RedeemPointsViewModel
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; }
        public int PointsBalance { get; set; }

        public List<RedemptionOption> AvailableOptions { get; set; }

        [Required]
        public int SelectedOptionId { get; set; }

        [Display(Name = "Utility Account Number")]
        [Required(ErrorMessage = "Please enter your municipal account number")]
        public string UtilityAccountNumber { get; set; }

        public string Message { get; set; }
    }

    public class RedemptionHistoryViewModel
    {
        public List<RedemptionRequest> Requests { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public decimal TotalDiscountsReceived { get; set; }
    }

    public class AdminRedemptionViewModel
    {
        public List<RedemptionRequest> PendingRequests { get; set; }
        public List<RedemptionRequest> ApprovedRequests { get; set; }
        public List<RedemptionRequest> RejectedRequests { get; set; }
        public RedemptionOption NewOption { get; set; }
        public List<RedemptionOption> ActiveOptions { get; set; }
    }
}