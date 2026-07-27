using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BinIT2WinIT.Models
{
    public class SmartBin
    {
        [Key]
        public int BinId { get; set; }

        [Required]
        public string BinName { get; set; }

        [Required]
        public string Location { get; set; }

        public int DropOffPointId { get; set; }

        // Sensor Data
        public int FillLevel { get; set; } = 0;
        public double CurrentWeight { get; set; } = 0;
        public double Capacity { get; set; } = 50;
        public string Status { get; set; } = "Online";
        public int BatteryLevel { get; set; } = 100;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Alert Flags
        public bool IsFullAlertSent { get; set; } = false;
        public bool IsMaintenanceAlertSent { get; set; } = false;

        // ✅ NEW: For temp officer assignment
        public int? TempAssignedOfficerId { get; set; }
        public DateTime? TempAssignmentDate { get; set; }
        public string TempAssignmentReason { get; set; }

        // Navigation
        [ForeignKey("DropOffPointId")]
        public virtual DropOffPoint DropOffPoint { get; set; }

        [ForeignKey("TempAssignedOfficerId")]
        public virtual CollectionOfficer TempAssignedOfficer { get; set; }
    }
}