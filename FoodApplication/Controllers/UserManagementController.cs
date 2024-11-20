using FoodApplication.DAL.Extend;
using FoodApplication.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodApplication.Web.Controllers
{
    public class UserManagementController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<UserManagementController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var users = new List<UserViewModel>();
            foreach (var user in await _userManager.Users.ToListAsync())
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                users.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsLocked = user.LockoutEnd > DateTime.UtcNow,
                    Roles = userRoles.ToList()
                });
            }

            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.ToListAsync();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                IsLocked = user.LockoutEnd > DateTime.UtcNow,
                Roles = allRoles.Select(r => new RoleSelectionViewModel
                {
                    RoleId = r.Id,
                    RoleName = r.Name,
                    IsSelected = userRoles.Contains(r.Name)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            user.Email = model.Email;
            user.UserName = model.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(model);
            }

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            var selectedRoles = model.Roles.Where(r => r.IsSelected).Select(r => r.RoleName).ToList();

            var rolesToRemove = currentRoles.Except(selectedRoles);
            var rolesToAdd = selectedRoles.Except(currentRoles);

            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            await _userManager.AddToRolesAsync(user, rolesToAdd);

            _logger.LogInformation("User {UserId} updated by {AdminId}", user.Id, User.FindFirstValue(ClaimTypes.NameIdentifier));
            TempData["Success"] = "User updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (user.LockoutEnd > DateTime.UtcNow)
            {
                // Unlock user
                await _userManager.SetLockoutEndDateAsync(user, null);
                _logger.LogInformation("User {UserId} unlocked by {AdminId}", user.Id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["Success"] = "User unlocked successfully";
            }
            else
            {
                // Lock user
                await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddYears(100));
                _logger.LogInformation("User {UserId} locked by {AdminId}", user.Id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["Success"] = "User locked successfully";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
