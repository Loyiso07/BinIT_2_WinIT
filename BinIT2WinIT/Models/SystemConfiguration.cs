using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BinIT2WinIT.Models
{
        public class SystemConfiguration
        {
            [Key]
            public int ConfigId { get; set; }

            [Required]
            public string ConfigKey { get; set; }

            [Required]
            public string ConfigValue { get; set; }

            public string Description { get; set; }
            public DateTime UpdatedDate { get; set; } = DateTime.Now;
            public string UpdatedBy { get; set; }
        }
    
}