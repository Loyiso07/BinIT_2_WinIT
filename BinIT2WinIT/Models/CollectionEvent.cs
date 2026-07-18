using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
        public class CollectionEvent
        {
            [Key]
            public int EventId { get; set; }

            [Required]
            public int DropOffPointId { get; set; }

            [Required]
            public DateTime EventDate { get; set; }

            [Required]
            public TimeSpan StartTime { get; set; }

            [Required]
            public TimeSpan EndTime { get; set; }

            public string Description { get; set; }
            public bool IsActive { get; set; } = true;

            [ForeignKey("DropOffPointId")]
            public virtual DropOffPoint DropOffPoint { get; set; }
        }
}