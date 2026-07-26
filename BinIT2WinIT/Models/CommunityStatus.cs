using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class CommunityStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required]
        public int DropOffPointId { get; set; }

        [Required]
        public string Status { get; set; } // Active, NeedsAttention, Critical, Inactive

        public string Notes { get; set; }

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public string UpdatedBy { get; set; }

        [ForeignKey("DropOffPointId")]
        public virtual DropOffPoint DropOffPoint { get; set; }
    }
}