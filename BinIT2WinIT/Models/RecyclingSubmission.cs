using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        public int? DropOffPointId { get; set; }  // ✅ MUST be nullable (int?)

        [Required]
        public double Weight { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; }

        public DateTime? VerifiedDate { get; set; }
        public int? VerifiedBy { get; set; }
        public string OfficerNotes { get; set; }

        // Navigation properties
        [ForeignKey("ResidentId")]
        public virtual Resident Resident { get; set; }

        [ForeignKey("MaterialTypeId")]
        public virtual MaterialType MaterialType { get; set; }

        [ForeignKey("DropOffPointId")]
        public virtual DropOffPoint DropOffPoint { get; set; }
    }
}