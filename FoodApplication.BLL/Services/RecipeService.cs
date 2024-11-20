using FoodApplication.BLL.Models;
using FoodApplication.DAL.Entities;
using FoodApplication.DAL.Extend;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IGenericRepository<Recipe> _recipeRepository;
        private readonly IGenericRepository<Ingredient> _ingredientRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFileService _fileService;

        public RecipeService(IGenericRepository<Recipe> recipeRepository,
                             IGenericRepository<Ingredient> ingredientRepository,
                             IGenericRepository<Category> categoryRepository,
                             UserManager<ApplicationUser> userManager
                            , IFileService _fileService
                             )
        {
            _recipeRepository = recipeRepository;
            _ingredientRepository = ingredientRepository;
            _categoryRepository = categoryRepository;
            _userManager = userManager;
            this._fileService = _fileService;
        }

        public async Task<IEnumerable<RecipeDTO>> GetAllRecipesAsync()
        {
            var recipes = await _recipeRepository.GetAllAsync();
            return recipes.Select(MapRecipeToDTO);
        }

        public async Task<RecipeDTO> GetRecipeByIdAsync(int id)
        {
            var recipe = await _recipeRepository.GetByIdAsync(id, include: r => r.Include(r => r.Categories)
                                                                         .Include(r => r.RecipeIngredients)
                                                                         .ThenInclude(ri => ri.Ingredient));
            return recipe != null ? MapRecipeToDTO(recipe) : null;
        }

        public async Task<int> CreateRecipeAsync(CreateRecipeDTO dto, string userId, string imagePath)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var recipe = new Recipe
            {
                Name = dto.Name,
                Description = dto.Description,
                Instructions = dto.Instructions,
                ImageUrl = imagePath,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = userId,
                CreatedByUser = user,
                Price = dto.Price,
                RecipeIngredients = new List<RecipeIngredient>(),
                Categories = await GetCategoriesFromIds(dto.CategoryIds ?? new List<int>())
            };

            if (dto.RecipeIngredients != null)
            {
                foreach (var riDto in dto.RecipeIngredients)
                {
                    var ingredient = await _ingredientRepository.GetByIdAsync(riDto.IngredientId);
                    if (ingredient == null) continue;

                    recipe.RecipeIngredients.Add(new RecipeIngredient
                    {
                        Recipe = recipe,
                        Ingredient = ingredient,
                        IngredientId = ingredient.Id,
                        Quantity = riDto.Quantity ?? "Not specified"
                    });
                }
            }

            await _recipeRepository.AddAsync(recipe);
            return recipe.Id;
        }

        public async Task UpdateRecipeAsync(int id, EditRecipeDTO dto, string webRootPath)
        {
            var recipe = await _recipeRepository.GetByIdAsync(id, include: r => r
                .Include(r => r.Categories)
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient));

            if (recipe == null)
                throw new KeyNotFoundException($"Recipe with ID {id} not found");

            recipe.Name = dto.Name;
            recipe.Description = dto.Description;
            recipe.Instructions = dto.Instructions;
            recipe.UpdatedAt = DateTime.UtcNow;

            if (dto.ImageFile != null)
            {
                var imagePath = await _fileService.SaveImageAsync(dto.ImageFile, webRootPath);
                if (!string.IsNullOrEmpty(recipe.ImageUrl))
                {
                    _fileService.DeleteImage(recipe.ImageUrl, webRootPath);
                }
                recipe.ImageUrl = imagePath;
            }

            await UpdateRecipeIngredients(recipe, dto.RecipeIngredients);
            await UpdateRecipeCategories(recipe, dto.CategoryIds);

            await _recipeRepository.UpdateAsync(recipe);
        }

        private async Task UpdateRecipeIngredients(Recipe recipe, List<RecipeIngredientDTO> ingredients)
        {
            recipe.RecipeIngredients.Clear();
            foreach (var riDto in ingredients)
            {
                var ingredient = await _ingredientRepository.GetByIdAsync(riDto.IngredientId);
                if (ingredient != null)
                {
                    recipe.RecipeIngredients.Add(new RecipeIngredient
                    {
                        Recipe = recipe,
                        Ingredient = ingredient,
                        IngredientId = ingredient.Id,
                        Quantity = riDto.Quantity ?? "Not specified"
                    });
                }
            }
        }

        private async Task UpdateRecipeCategories(Recipe recipe, List<int> categoryIds)
        {
            recipe.Categories.Clear();
            foreach (var categoryId in categoryIds)
            {
                var category = await _categoryRepository.GetByIdAsync(categoryId);
                if (category != null)
                {
                    recipe.Categories.Add(category);
                }
            }
        }

        public async Task DeleteRecipeAsync(int id)
        {
            await _recipeRepository.DeleteAsync(id);
        }

        private async Task<ICollection<RecipeIngredient>> CreateRecipeIngredients(List<RecipeIngredientDTO> recipeIngredientDtos)
        {
            var recipeIngredients = new List<RecipeIngredient>();

            foreach (var riDto in recipeIngredientDtos)
            {
                var ingredient = await _ingredientRepository.GetByIdAsync(riDto.IngredientId);
                if (ingredient == null) throw new KeyNotFoundException($"Ingredient with id {riDto.IngredientId} not found");

                var recipeIngredient = new RecipeIngredient
                {
                    Ingredient = ingredient,
                    IngredientId = ingredient.Id,
                    Quantity = riDto.Quantity

                };
                riDto.IngredientName = ingredient.Name;
                recipeIngredients.Add(recipeIngredient);
            }
            return recipeIngredients;
        }

        private async Task<ICollection<Category>> GetCategoriesFromIds(List<int> ids)
        {
            var categories = new List<Category>();
            foreach (var id in ids)
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category != null)
                {
                    categories.Add(category);
                }
            }
            return categories;

        }
        public async Task<bool> AddRecipeToFavoritesAsync(int recipeId, string userId)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId, include: q => q.Include(r => r.FavoriteByUsers));
            if (recipe == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            if (!recipe.FavoriteByUsers.Any(u => u.Id == userId))
            {
                recipe.FavoriteByUsers.Add(user);
                await _recipeRepository.UpdateAsync(recipe);
            }

            return true;
        }

        public async Task<bool> ToggleFavoriteAsync(int recipeId, string userId)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId, include: q => q.Include(r => r.FavoriteByUsers));
            if (recipe == null) return false;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var isFavorited = recipe.FavoriteByUsers.Any(u => u.Id == userId);
            if (isFavorited)
            {
                recipe.FavoriteByUsers.Remove(user);
            }
            else
            {
                recipe.FavoriteByUsers.Add(user);
            }

            await _recipeRepository.UpdateAsync(recipe);
            return !isFavorited;  // Returns true if it was added to favorites, false if it was removed
        }

        public async Task<bool> RemoveRecipeFromFavoritesAsync(int recipeId, string userId)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId, include: q => q.Include(r => r.FavoriteByUsers));
            if (recipe == null) return false;

            var user = recipe.FavoriteByUsers.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                recipe.FavoriteByUsers.Remove(user);
                await _recipeRepository.UpdateAsync(recipe);
                return true;
            }

            return false;
        }

        private RecipeDTO MapRecipeToDTO(Recipe recipe)
        {
            return new RecipeDTO
            {
                Id = recipe.Id,
                Name = recipe.Name ?? "Unnamed Recipe",
                Description = recipe.Description ?? "No Description",
                Instructions = recipe.Instructions ?? "No Instructions",

                CreatedAt = recipe.CreatedAt ?? DateTime.MinValue,
                UpdatedAt = recipe.UpdatedAt,
                Price = recipe.Price ?? 0.00m,
                CreatedByUserId = recipe.CreatedByUserId,
                CreatedByUserName = recipe.CreatedByUser?.UserName ?? "Unknown",
                FavoriteByUserIds = recipe.FavoriteByUsers?.Select(u => u.Id).ToList() ?? new List<string>(),
                FavoriteCount = recipe.FavoriteByUsers?.Count ?? 0,
                RecipeIngredients = recipe.RecipeIngredients?.Select(ri => new RecipeIngredientDTO
                {
                    IngredientId = ri.IngredientId,
                    IngredientName = ri.Ingredient?.Name ?? "Unknown Ingredient",
                    Quantity = ri.Quantity ?? "Unknown Quantity"
                }).ToList() ?? new List<RecipeIngredientDTO>(),
                CategoryIds = recipe.Categories?.Select(c => c.Id).ToList() ?? new List<int>(),
                CategoryNames = recipe.Categories?.Select(c => c.Name).ToList() ?? new List<string>(),
                ImageUrl = !string.IsNullOrEmpty(recipe.ImageUrl)
                    ? $"/{recipe.ImageUrl}"  // Prepend a forward slash
                    : "/images/default-recipe.jpg"
            };
        }


    }
    public interface IRecipeService
    {
        Task<IEnumerable<RecipeDTO>> GetAllRecipesAsync();
        Task<RecipeDTO> GetRecipeByIdAsync(int id);
        Task<int> CreateRecipeAsync(CreateRecipeDTO dto, string userId, string imagePath);
        Task UpdateRecipeAsync(int id, EditRecipeDTO dto, string webRootPath);
        Task DeleteRecipeAsync(int id);
        Task<bool> ToggleFavoriteAsync(int recipeId, string userId);

        Task<bool> AddRecipeToFavoritesAsync(int recipeId, string userId);
        Task<bool> RemoveRecipeFromFavoritesAsync(int recipeId, string userId);

    }
}