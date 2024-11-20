using FoodApplication.BLL.Models;

namespace FoodApplication.Web.Models
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? ProfileImageUrl { get; set; }  
        public List<string> Roles { get; set; }
        public int CreatedRecipes { get; set; }
        public int FavoriteRecipes { get; set; }
        public int TotalOrders { get; set; }
        public List<RecipeDTO> CreatedRecipesList { get; set; }
        public List<RecipeDTO> FavoriteRecipesList { get; set; }
        public List<OrderDTO> OrderHistory { get; set; }
    }
}
