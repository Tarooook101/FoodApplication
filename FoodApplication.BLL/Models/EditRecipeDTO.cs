using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Models
{
    public class EditRecipeDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Recipe name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Recipe name must be between 3 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Instructions are required")]
        public string Instructions { get; set; }

        [Required(ErrorMessage = "At least one ingredient is required")]
        public List<RecipeIngredientDTO> RecipeIngredients { get; set; } = new();

        [Required(ErrorMessage = "At least one category is required")]
        public List<int> CategoryIds { get; set; } = new();

        public IFormFile? ImageFile { get; set; }
        public string? ExistingImageUrl { get; set; }
        public string? ImagePreview { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be between 0.01 and 999,999.99")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }
    }
}
