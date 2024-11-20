using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Models
{
    public class ResetPasswordDTO
    {
        [Required(ErrorMessage = "Password Required")]
        [MinLength(6, ErrorMessage = "min length is 6 char")]
        [MaxLength(10, ErrorMessage = "Max length is 10 char")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password Required")]
        [MinLength(6, ErrorMessage = "min length is 6 char")]
        [MaxLength(10, ErrorMessage = "Max length is 10 char")]
        [Compare("Password", ErrorMessage = "Password not match")]
        public string ConfirmPassword { get; set; }

        public string Email { get; set; }
        public string Token { get; set; }
    }
}
