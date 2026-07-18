using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BinIT2WinIT.Models
{
    public class RecyclingSubmission
    { 
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int ResidentId { get; set; }

        [Required]
        public int MaterialTypeId { get; set; }

        [Required]
        public int DropOffPointId { get; set; }

        [Required]
        [Range(0.1, 1000)]
        public double Weight { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Pending";

        public string OfficerNotes { get; set; }

        public int? VerifiedBy { get; set; }
        public DateTime? VerifiedDate { get; set; }

        public bool IsFlagged { get; set; } = false;
        public string FlagReason { get; set; }

        [ForeignKey("ResidentId")]
        public virtual Resident Resident { get; set; }

        [ForeignKey("MaterialTypeId")]
        public virtual MaterialType MaterialType { get; set; }

        [ForeignKey("DropOffPointId")]
        public virtual DropOffPoint DropOffPoint { get; set; }

        [ForeignKey("VerifiedBy")]
        public virtual CollectionOfficer VerifyingOfficer { get; set; }
    }
 }

