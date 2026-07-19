using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BinIT2WinIT.Models  // ← Your project namespace
{
    public class RecyclingSubmissionViewModel
    {
        [Required(ErrorMessage = "Please select a material type.")]
        public int MaterialTypeId { get; set; }

        [Required(ErrorMessage = "Please select a drop-off point.")]
        public int DropOffPointId { get; set; }

        [Required(ErrorMessage = "Please enter the weight.")]
        [Range(0.1, 1000, ErrorMessage = "Weight must be between 0.1 and 1000 kg.")]
        public double Weight { get; set; }

        public List<MaterialType> MaterialTypes { get; set; }
        public List<DropOffPoint> DropOffPoints { get; set; }
    }
}