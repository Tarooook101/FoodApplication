using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Models
{
    public class RecipeIngredientDTO
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Quantity { get; set; }
    }
}
