using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class AdminCreationAudit
    {
        [Key]
        public int AuditId { get; set; }

        [Required]
        public string NewAdminUserId { get; set; }

        [Required]
        public string CreatedByUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string NewAdminEmail { get; set; }
        public string NewAdminName { get; set; }
        public string CreatedByName { get; set; }

        [ForeignKey("NewAdminUserId")]
        public virtual ApplicationUser NewAdmin { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual ApplicationUser CreatedBy { get; set; }
    }
}