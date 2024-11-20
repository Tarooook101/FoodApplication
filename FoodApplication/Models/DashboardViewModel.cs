using FoodApplication.BLL.Models;

namespace FoodApplication.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalRecipes { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public List<OrderDTO> RecentOrders { get; set; }
        public List<RecipeDTO> TopRecipes { get; set; }
    }
}
