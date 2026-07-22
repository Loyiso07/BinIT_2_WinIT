using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class CollectionOfficer
    {
        [Key]
        public int OfficerId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string FullName { get; set; }

        public string PhoneNumber { get; set; } = "000-000-0000";

        // ✅ THIS IS THE FOREIGN KEY TO THE REGION/DROP-OFF POINT
        public int? DropOffPointId { get; set; }

        public string EmployeeNumber { get; set; }
        public string Department { get; set; } = "Waste Management";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        // ✅ NAVIGATION PROPERTY TO THE REGION
        [ForeignKey("DropOffPointId")]
        public virtual DropOffPoint AssignedDropOffPoint { get; set; }

        public virtual ICollection<RecyclingSubmission> VerifiedSubmissions { get; set; }
    }
}