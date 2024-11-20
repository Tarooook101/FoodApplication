using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Models
{
    public class IngredientDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingredient name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Ingredient name must be between 2 and 50 characters")]
        public string Name { get; set; }
    }
}
