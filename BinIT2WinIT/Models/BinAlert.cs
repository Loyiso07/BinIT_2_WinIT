using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class BinAlert
    {
        [Key]
        public int AlertId { get; set; }

        [Required]
        public int BinId { get; set; }

        public string AlertType { get; set; } // Full, Maintenance, Urgent

        public string Message { get; set; }

        public bool IsResolved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ResolvedAt { get; set; }

        public string ResolvedBy { get; set; }

        public int? AssignedOfficerId { get; set; }

        // ✅ Navigation Properties
        [ForeignKey("BinId")]
        public virtual SmartBin SmartBin { get; set; }

        [ForeignKey("AssignedOfficerId")]
        public virtual CollectionOfficer AssignedOfficer { get; set; }
    }
}