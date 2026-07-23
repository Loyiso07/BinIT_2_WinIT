using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace BinIT2WinIT.Models
{
    public class AssignOfficerViewModel
    {
        public int OfficerId { get; set; }
        public string OfficerName { get; set; }

        [Display(Name = "Assigned Drop-Off Point")]
        public int? DropOffPointId { get; set; }

        public SelectList DropOffPoints { get; set; }
    }
}