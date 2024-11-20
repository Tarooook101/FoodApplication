using FoodApplication.BLL.Services;
using FoodApplication.DAL.Extend;
using FoodApplication.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodApplication.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRecipeService _recipeService;
        private readonly IOrderService _orderService;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            IRecipeService recipeService,
            IOrderService orderService,
            IFileService fileService,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _recipeService = recipeService;
            _orderService = orderService;
            _fileService = fileService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var recipes = await _recipeService.GetAllRecipesAsync();
            var createdRecipes = recipes.Where(r => r.CreatedByUserId == userId).ToList();
            var favoriteRecipes = recipes.Where(r => r.FavoriteByUserIds.Contains(userId)).ToList();
            var orders = await _orderService.GetOrderHistoryForUserAsync(userId);

            var viewModel = new UserProfileViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                CreatedRecipes = createdRecipes.Count,
                FavoriteRecipes = favoriteRecipes.Count,
                TotalOrders = orders.Count(),
                CreatedRecipesList = createdRecipes,
                FavoriteRecipesList = favoriteRecipes,
                OrderHistory = orders.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfileImage(IFormFile profileImage)
        {
            try
            {
                if (profileImage == null)
                    return Json(new { success = false, message = "No image was provided." });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    return Json(new { success = false, message = "User not found." });

                // Delete old profile image if exists
                if (!string.IsNullOrEmpty(user.ProfileImageUrl))
                {
                    _fileService.DeleteImage(user.ProfileImageUrl, _webHostEnvironment.WebRootPath);
                }

                // Save new profile image
                var imagePath = await _fileService.SaveImageAsync(profileImage, _webHostEnvironment.WebRootPath);

                // Update user profile image URL
                user.ProfileImageUrl = imagePath;
                await _userManager.UpdateAsync(user);

                return Json(new { success = true, imageUrl = $"/{imagePath}" });
            }
            catch (ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while uploading the image." });
            }
        }
    }
}
