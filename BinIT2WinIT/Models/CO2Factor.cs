using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BinIT2WinIT.Models
{
         public class CO2Factor
        {
            [Key]
            public int CO2FactorId { get; set; }

            [Required]
            public int MaterialTypeId { get; set; }

            [Required]
            [Range(0.001, 100)]
            public double CO2SavedPerKg { get; set; }

            public DateTime EffectiveDate { get; set; } = DateTime.Now;
            public DateTime? EndDate { get; set; }
            public bool IsActive { get; set; } = true;

            [ForeignKey("MaterialTypeId")]
            public virtual MaterialType MaterialType { get; set; }
         }
 }
