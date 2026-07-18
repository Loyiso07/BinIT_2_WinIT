using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BinIT2WinIT.Models
{
        public class MaterialType
        {
            [Key]
            public int MaterialTypeId { get; set; }

            [Required]
            public string Name { get; set; }

            public string Description { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public virtual ICollection<RecyclingSubmission> Submissions { get; set; }
            public virtual ICollection<PointsRate> PointsRates { get; set; }
            public virtual ICollection<CO2Factor> CO2Factors { get; set; }
        }
    }