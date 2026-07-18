using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BinIT2WinIT.Models
{ 
        public class DropOffPoint
        {
            [Key]
            public int DropOffPointId { get; set; }

            [Required]
            public string Name { get; set; }

            [Required]
            public string Address { get; set; }

            public string City { get; set; }
            public string Suburb { get; set; }

            public string ContactPerson { get; set; }
            public string PhoneNumber { get; set; }

            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public virtual ICollection<CollectionOfficer> Officers { get; set; }
            public virtual ICollection<RecyclingSubmission> Submissions { get; set; }
            public virtual ICollection<CollectionEvent> CollectionEvents { get; set; }
        }
    }