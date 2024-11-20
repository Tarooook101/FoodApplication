using FoodApplication.BLL.Services;
using FoodApplication.DAL.Entities;
using FoodApplication.DAL.Extend;
using FoodApplication.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodApplication.Web.Controllers
{
	public class DashboardController : Controller
	{
        private readonly IRecipeService _recipeService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICategoryService _categoryService;

        public DashboardController(
            IRecipeService recipeService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            ICategoryService categoryService)
        {
            _recipeService = recipeService;
            _orderService = orderService;
            _userManager = userManager;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var totalRecipes = (await _recipeService.GetAllRecipesAsync()).Count();
            var totalOrders = (await _orderService.GetAllOrdersAsync()).Count();
            var totalUsers = await _userManager.Users.CountAsync();
            var totalCategories = (await _categoryService.GetAllCategoriesAsync()).Count();

            var recentOrders = (await _orderService.GetAllOrdersAsync())
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            var topRecipes = (await _recipeService.GetAllRecipesAsync())
                .OrderByDescending(r => r.FavoriteCount)
                .Take(5)
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalRecipes = totalRecipes,
                TotalOrders = totalOrders,
                TotalUsers = totalUsers,
                TotalCategories = totalCategories,
                RecentOrders = recentOrders,
                TopRecipes = topRecipes
            };

            return View(viewModel);
        }
    }
}
