using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Models
{
    public class ProfileImageUpdateDTO
    {
        public string UserId { get; set; }
        public string ImageUrl { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}
