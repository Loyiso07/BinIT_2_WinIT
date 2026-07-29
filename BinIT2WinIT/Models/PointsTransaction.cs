using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class PointsTransaction
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int ResidentId { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        // Changed from int to decimal for better precision
        [Required]
        public decimal Amount { get; set; }  // ✅ Works with default precision

        [Required]
        public string Description { get; set; }

        [Required]
        public string Type { get; set; }

        public int? ReferenceId { get; set; }
        public string Reason { get; set; }

        [ForeignKey("ResidentId")]
        public virtual Resident Resident { get; set; }
    }
}