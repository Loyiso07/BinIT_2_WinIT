using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BinIT2WinIT.Models
{
         public class ApplicationUser : IdentityUser
        {
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string Suburb { get; set; }
            public string City { get; set; }
            public bool IsActive { get; set; } = true;
            public DateTime CreatedAt { get; set; } = DateTime.Now;
        }
    
}