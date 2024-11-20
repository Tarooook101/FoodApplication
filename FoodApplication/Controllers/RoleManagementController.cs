using FoodApplication.BLL.Models;
using FoodApplication.DAL.Extend;
using FoodApplication.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FoodApplication.Web.Controllers
{
    public class RoleManagementController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RoleManagementController> _logger;

        public RoleManagementController(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<RoleManagementController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var roles = new List<RoleViewModel>();
            foreach (var role in await _roleManager.Roles.ToListAsync())
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                roles.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    CreatedOn = role.CreatedOn,
                    CreatedBy = role.CreatedBy,
                    IsActive = role.IsActive ?? true,
                    UserCount = usersInRole.Count
                });
            }
            return View(roles);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ApplicationRole { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationRole role)
        {
            if (!ModelState.IsValid)
                return View(role);

            role.CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            role.CreatedOn = DateTime.Now.ToShortDateString();

            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role {RoleName} created by {UserId}", role.Name, role.CreatedBy);
                TempData["Success"] = "Role created successfully";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(role);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationRole role)
        {
            if (!ModelState.IsValid)
                return View(role);

            var existingRole = await _roleManager.FindByIdAsync(role.Id);
            if (existingRole == null)
                return NotFound();

            existingRole.Name = role.Name;
            existingRole.IsActive = role.IsActive;

            var result = await _roleManager.UpdateAsync(existingRole);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role {RoleId} updated by {UserId}", role.Id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["Success"] = "Role updated successfully";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
                return NotFound();

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role {RoleId} deleted by {UserId}", id, User.FindFirstValue(ClaimTypes.NameIdentifier));
                TempData["Success"] = "Role deleted successfully";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Error deleting role. Please ensure no users are assigned to this role.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageUsers(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound();

            var users = new List<UserInRoleDTO>();
            foreach (var user in await _userManager.Users.ToListAsync())
            {
                users.Add(new UserInRoleDTO
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                });
            }

            ViewBag.RoleId = roleId;
            ViewBag.RoleName = role.Name;
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUsers(List<UserInRoleDTO> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound();

            foreach (var userRole in model)
            {
                var user = await _userManager.FindByIdAsync(userRole.UserId);
                if (user != null)
                {
                    if (userRole.IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                        await _userManager.AddToRoleAsync(user, role.Name);
                    else if (!userRole.IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                        await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }

            TempData["Success"] = "User roles updated successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
