using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.DAL.Entities
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        [ForeignKey("RecipeId")]

        public Recipe Recipe { get; set; }
        public int IngredientId { get; set; }
        [ForeignKey("IngredientId")]
        public Ingredient Ingredient { get; set; }
        public string Quantity { get; set; }
    }
}
