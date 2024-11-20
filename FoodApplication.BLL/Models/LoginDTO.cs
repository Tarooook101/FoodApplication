using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Models
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Email Required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password Required")]
        [MinLength(6, ErrorMessage = "min length is 6 char")]
        [MaxLength(10, ErrorMessage = "Max length is 10 char")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
