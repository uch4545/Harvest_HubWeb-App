using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace HarvestHub.WebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string CNIC { get; set; }
        public string RoleType { get; set; }

        // 👇 Navigation to Buyer
        public Buyer Buyer { get; set; }

        // 👇 Navigation to Farmer (if needed)
        public Farmer Farmer { get; set; }
        public string OtpCode { get; set; }
        public DateTime OtpExpiry { get; set; }
    }
}
