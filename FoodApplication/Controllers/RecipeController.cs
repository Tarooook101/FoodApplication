using FoodApplication.BLL.Models;
using FoodApplication.BLL.Services;
using FoodApplication.DAL.Extend;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodApplication.Web.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IRecipeService _recipeService;
        private readonly IIngredientService _ingredientService;
        private readonly ICategoryService _categoryService;
        private readonly IFileService _fileService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecipeController(IWebHostEnvironment webHostEnvironment, IRecipeService recipeService, IIngredientService _ingredientService,
            ICategoryService _categoryService, UserManager<ApplicationUser> userManager, IFileService _fileService)
        {
            _recipeService = recipeService;
            this._ingredientService = _ingredientService;
            this._categoryService = _categoryService;
            this._fileService = _fileService;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipes = await _recipeService.GetAllRecipesAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.CurrentUserId = userId;
            return View(recipes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
                return NotFound();


            Console.WriteLine($"Recipe: {recipe.Name}");
            Console.WriteLine($"Ingredients: {string.Join(", ", recipe.RecipeIngredients.Select(ri => ri.IngredientName))}");
            Console.WriteLine($"Categories: {string.Join(", ", recipe.CategoryNames)}");
            return View(recipe);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Ingredients = await _ingredientService.GetAllIngredientsAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(new CreateRecipeDTO());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRecipeDTO createRecipeDto)
        {
            if (!ModelState.IsValid)
            {
                await PrepareViewBagForRecipeForm();
                return View(createRecipeDto);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User is not authenticated.");
                await PrepareViewBagForRecipeForm();
                return View(createRecipeDto);
            }

            string imagePath = null;
            // Handle image upload
            if (createRecipeDto.ImageFile != null)
            {
                try
                {
                    imagePath = await _fileService.SaveImageAsync(createRecipeDto.ImageFile, _webHostEnvironment.WebRootPath);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("ImageFile", ex.Message);
                    await PrepareViewBagForRecipeForm();
                    return View(createRecipeDto);
                }
            }
            else
            {
                imagePath = "images/default-recipe.jpg";
            }

            try
            {
                var recipeId = await _recipeService.CreateRecipeAsync(createRecipeDto, userId, imagePath);
                return RedirectToAction(nameof(Details), new { id = recipeId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating recipe: {ex.Message}");
                await PrepareViewBagForRecipeForm();
                return View(createRecipeDto);
            }
        }

        private async Task PrepareViewBagForRecipeForm()
        {
            ViewBag.Ingredients = await _ingredientService.GetAllIngredientsAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
                return NotFound();

            var editRecipeDto = new EditRecipeDTO
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                Instructions = recipe.Instructions,
                RecipeIngredients = recipe.RecipeIngredients,
                CategoryIds = recipe.CategoryIds,
                ExistingImageUrl = recipe.ImageUrl,
                ImagePreview = recipe.ImageUrl
            };

            await PrepareViewBagForRecipeForm();
            return View(editRecipeDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditRecipeDTO editRecipeDto)
        {
            if (id != editRecipeDto.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await PrepareViewBagForRecipeForm();
                return View(editRecipeDto);
            }

            try
            {
                await _recipeService.UpdateRecipeAsync(id, editRecipeDto, _webHostEnvironment.WebRootPath);
                TempData["SuccessMessage"] = "Recipe updated successfully";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating recipe: {ex.Message}");
                await PrepareViewBagForRecipeForm();
                return View(editRecipeDto);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
                return NotFound();
            return View(recipe);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _recipeService.DeleteRecipeAsync(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFavorite(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            bool isFavorited = await _recipeService.ToggleFavoriteAsync(recipeId, userId);
            var updatedRecipe = await _recipeService.GetRecipeByIdAsync(recipeId);

            return Json(new
            {
                success = true,
                isFavorited = isFavorited,
                favoriteCount = updatedRecipe.FavoriteCount
            });
        }


        [HttpPost]
        public async Task<IActionResult> AddToFavorites(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _recipeService.AddRecipeToFavoritesAsync(recipeId, userId);
            return Json(new { success = result });
        }
        [HttpPost]
        public async Task<IActionResult> RemoveFromFavorites(int recipeId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _recipeService.RemoveRecipeFromFavoritesAsync(recipeId, userId);
            return Json(new { success = result });
        }
    }
}