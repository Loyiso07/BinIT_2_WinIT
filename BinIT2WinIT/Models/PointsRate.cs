using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BinIT2WinIT.Models
{
    public class PointsRate
    {
        [Key]
        public int PointsRateId { get; set; }

        [Required]
        public int MaterialTypeId { get; set; }

        [Required]
        [Range(0.1, 100)]
        public double PointsPerKg { get; set; }

        public DateTime EffectiveDate { get; set; } = DateTime.Now;
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey("MaterialTypeId")]
        public virtual MaterialType MaterialType { get; set; }
    }
}

