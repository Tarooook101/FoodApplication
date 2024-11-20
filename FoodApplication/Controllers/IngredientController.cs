using FoodApplication.BLL.Models;
using FoodApplication.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace FoodApplication.Web.Controllers
{
    public class IngredientController : Controller
    {
        private readonly IIngredientService _ingredientService;

        public IngredientController(IIngredientService ingredientService)
        {
            _ingredientService = ingredientService;
        }

        public async Task<IActionResult> Index()
        {
            var ingredients = await _ingredientService.GetAllIngredientsAsync();
            return View(ingredients);
        }

        public async Task<IActionResult> Details(int id)
        {
            var ingredient = await _ingredientService.GetIngredientByIdAsync(id);
            if (ingredient == null)
                return NotFound();
            return View(ingredient);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IngredientDTO ingredientDto)
        {
            if (ModelState.IsValid)
            {
                await _ingredientService.CreateIngredientAsync(ingredientDto);
                return RedirectToAction(nameof(Index));
            }
            return View(ingredientDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var ingredient = await _ingredientService.GetIngredientByIdAsync(id);
            if (ingredient == null)
                return NotFound();
            return View(ingredient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IngredientDTO ingredientDto)
        {
            if (ModelState.IsValid)
            {
                await _ingredientService.UpdateIngredientAsync(id, ingredientDto);
                return RedirectToAction(nameof(Index));
            }
            return View(ingredientDto);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var ingredient = await _ingredientService.GetIngredientByIdAsync(id);
            if (ingredient == null)
                return NotFound();
            return View(ingredient);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _ingredientService.DeleteIngredientAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
