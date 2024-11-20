using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.DAL.Extend
{
    public class ApplicationRole : IdentityRole
    {
        public ApplicationRole()
        {
            IsActive = true;
            CreatedOn = DateTime.Now.ToShortDateString();
        }

        public bool? IsActive { get; set; }
        public string? CreatedOn { get; set; }

        public string? CreatedBy { get; set; } 
    }
}
