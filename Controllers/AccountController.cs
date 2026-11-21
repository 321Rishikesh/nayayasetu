using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;
using NyayaSetu.ViewModels;

namespace NyayaSetu.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuditService _audit;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuditService audit)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _audit = audit;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Invalid credentials or account deactivated.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
            {
                await _audit.LogAsync(user.Id, user.Email, "Login", "Account", user.Id, "Successful login", HttpContext.Connection.RemoteIpAddress?.ToString());

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                if (await _userManager.IsInRoleAsync(user, AppRoles.SuperAdmin))
                    return RedirectToAction("Dashboard", "Admin");
                if (await _userManager.IsInRoleAsync(user, AppRoles.DepartmentOfficer))
                    return RedirectToAction("Dashboard", "Officer");
                if (await _userManager.IsInRoleAsync(user, AppRoles.LegalAidOfficer))
                    return RedirectToAction("Dashboard", "LegalAid");

                return RedirectToAction("Dashboard", "Citizen");
            }

            ModelState.AddModelError(string.Empty, result.IsLockedOut
                ? "Account temporarily locked after repeated failed attempts. Please try again in 10 minutes."
                : "Invalid email or password.");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                PhoneNumber = model.PhoneNumber?.Trim(),
                City = model.City?.Trim(),
                State = model.State?.Trim(),
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, AppRoles.Citizen);
                await _signInManager.SignInAsync(user, isPersistent: false);
                await _audit.LogAsync(user.Id, user.Email, "Register", "Account", user.Id, "New citizen registered");
                TempData["Success"] = "Welcome to NyayaSetu! Your citizen account is ready.";
                return RedirectToAction("Dashboard", "Citizen");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = _userManager.GetUserId(User);
            await _signInManager.SignOutAsync();
            await _audit.LogAsync(userId, User.Identity?.Name, "Logout", "Account", userId, "User signed out");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                _ = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _audit.LogAsync(user.Id, user.Email, "ForgotPasswordRequested", "Account", user.Id, "Password reset workflow initiated (academic prototype demo).");
            }
            TempData["Success"] = "If an account exists for that email, password recovery instructions have been generated (academic prototype mode — no external SMTP configured).";
            return RedirectToAction(nameof(Login));
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            return View(new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PinCode = user.PinCode,
                PreferredLanguage = user.PreferredLanguage
            });
        }

        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(Login));

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();
            user.Address = model.Address?.Trim();
            user.City = model.City?.Trim();
            user.State = model.State?.Trim();
            user.PinCode = model.PinCode?.Trim();
            user.PreferredLanguage = model.PreferredLanguage;

            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Profile));
        }

        public IActionResult AccessDenied() => View();
    }
}
