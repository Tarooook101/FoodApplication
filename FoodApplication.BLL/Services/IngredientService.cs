using FoodApplication.BLL.Models;
using FoodApplication.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodApplication.BLL.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IGenericRepository<Ingredient> _ingredientRepository;

        public IngredientService(IGenericRepository<Ingredient> ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        public async Task<IEnumerable<IngredientDTO>> GetAllIngredientsAsync()
        {
            var ingredients = await _ingredientRepository.GetAllAsync();
            return ingredients.Select(MapIngredientToDTO);
        }

        public async Task<IngredientDTO> GetIngredientByIdAsync(int id)
        {
            var ingredient = await _ingredientRepository.GetByIdAsync(id);
            return ingredient != null ? MapIngredientToDTO(ingredient) : null;
        }

        public async Task<int> CreateIngredientAsync(IngredientDTO dto)
        {
            var ingredient = new Ingredient
            {
                Name = dto.Name
            };

            await _ingredientRepository.AddAsync(ingredient);
            return ingredient.Id;
        }

        public async Task UpdateIngredientAsync(int id, IngredientDTO dto)
        {
            var ingredient = await _ingredientRepository.GetByIdAsync(id);
            if (ingredient == null) throw new KeyNotFoundException("Ingredient not found");

            ingredient.Name = dto.Name;
            await _ingredientRepository.UpdateAsync(ingredient);
        }

        public async Task DeleteIngredientAsync(int id)
        {
            await _ingredientRepository.DeleteAsync(id);
        }

        private IngredientDTO MapIngredientToDTO(Ingredient ingredient)
        {
            return new IngredientDTO
            {
                Id = ingredient.Id,
                Name = ingredient.Name
            };
        }
    }
    public interface IIngredientService
    {
        Task<IEnumerable<IngredientDTO>> GetAllIngredientsAsync();
        Task<IngredientDTO> GetIngredientByIdAsync(int id);
        Task<int> CreateIngredientAsync(IngredientDTO dto);
        Task UpdateIngredientAsync(int id, IngredientDTO dto);
        Task DeleteIngredientAsync(int id);
    }
}
