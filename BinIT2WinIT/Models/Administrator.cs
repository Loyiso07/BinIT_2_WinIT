using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
        public class Administrator
        {
            [Key]
            public int AdminId { get; set; }

            [Required]
            public string UserId { get; set; }

            [Required]
            public string FullName { get; set; }

            [Required]
            public string Email { get; set; }

            public string Department { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; } = DateTime.Now;

            [ForeignKey("UserId")]
            public virtual ApplicationUser User { get; set; }
        }
    }