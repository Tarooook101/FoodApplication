using FoodApplication.BLL.Models;
using FoodApplication.DAL.Extend;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FoodApplication.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        #region Registration

        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registration(RegistrationDTO model)
        {
            var user = new ApplicationUser()
            {
                UserName = model.FullName,
                Email = model.Email,
                IsAgree = model.IsAgree
            };

            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return RedirectToAction("Login");
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }

            return View(model);
        }


        #endregion

        #region Login

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO login)
        {
            // 1  (Check on user)
            var user = await userManager.FindByEmailAsync(login.Email);
            // false : في حالة لو عمل password غلط ثلا فلو القيمة ده كانت ترو هيتقفل المستخدم ده فإحنا كده عملناها غلط
            var result = await signInManager.PasswordSignInAsync(user, login.Password, login.RememberMe, false);

            if (result.Succeeded)
            {
                // Index, Home
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ModelState.AddModelError("", "Invalid UserName Or Password");
            }

            return View(login);
        }

        #endregion

        #region Sign Out

        [HttpPost]
        public async Task<IActionResult> LogOff()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        #endregion

        #region Forget Password

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }


        [HttpGet]
        public IActionResult ConfirmForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDTO model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                // 1) generate token
                var token = await userManager.GeneratePasswordResetTokenAsync(user);

                // 2) prepare password reset link
                var passwordResetLink = Url.Action("ResetPassword", "Account", new { Email = model.Email, Token = token }, Request.Scheme);

                EventLog log = new EventLog();

                log.Source = "HR System";
                log.WriteEntry(passwordResetLink, EventLogEntryType.Information);


                return RedirectToAction("ConfirmForgetPassword");
            }

            return View(model);
        }


        #endregion

        #region Reset Password

        [HttpGet]
        public IActionResult ResetPassword(string Email, string Token)
        {
            return View();
        }


        [HttpGet]
        public IActionResult ConfirmResetPassword()
        {
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user != null)
            {
                var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("ConfirmResetPassword");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

            return View(model);
        }

        #endregion

    }
}
