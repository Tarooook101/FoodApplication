using FoodApplication.BLL.Models;
using FoodApplication.BLL.Services;
using FoodApplication.DAL.Entities;
using FoodApplication.DAL.Extend;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FoodApplication.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IRecipeService _recipeService;
        private readonly IIngredientService _ingredientService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService _orderService, IRecipeService _recipeService, IIngredientService _ingredientService,
            ICategoryService _categoryService, UserManager<ApplicationUser> userManager)
        {
            this._orderService = _orderService;
            this._recipeService = _recipeService;
            this._ingredientService = _ingredientService;
            this._categoryService = _categoryService;
            _userManager = userManager;

        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderDetailsAsync(id);
            if (order == null)
                return NotFound();
            return View(order);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Recipes = await _recipeService.GetAllRecipesAsync();
            ViewBag.Ingredients = await _ingredientService.GetAllIngredientsAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(new CreateOrderDTO { OrderDate = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOrderDTO createOrderDto)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                    return Unauthorized();

                try
                {
                    var orderId = await _orderService.CreateOrderAsync(createOrderDto, user.Id);
                    TempData["SuccessMessage"] = "Order created successfully!";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating order: " + ex.Message);
                }
            }

            // If we got this far, something failed; redisplay form
            ViewBag.Recipes = await _recipeService.GetAllRecipesAsync();
            ViewBag.Ingredients = await _ingredientService.GetAllIngredientsAsync();
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
            return View(createOrderDto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            await _orderService.UpdateOrderStatusAsync(id, status);
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> UserHistory(int userId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var orders = await _orderService.GetOrderHistoryForUserAsync(user.Id);
            return View(orders);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var orderDto = await _orderService.GetOrderForDeleteAsync(id);
            if (orderDto == null)
            {
                return NotFound();
            }
            return View(orderDto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _orderService.DeleteOrderAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
