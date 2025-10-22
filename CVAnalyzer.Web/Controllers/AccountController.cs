using CVAnalyzer.Application.DTOs.Auth;
using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Entities;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace CVAnalyzer.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            IAuthService authService,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _authService = authService;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Find user
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null || !user.IsActive)
                {
                    TempData["ErrorMessage"] = "Invalid email or password";
                    return View(model);
                }

                // Verify password
                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.Password);

                if (!passwordCheck)
                {
                    TempData["ErrorMessage"] = "Invalid email or password";
                    return View(model);
                }

                // Sign in with cookie authentication (THIS IS THE KEY!)
                await _signInManager.SignInAsync(user, isPersistent: model.RememberMe);

                // Update last login
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Call auth service for JWT (optional - for API calls)
                var result = await _authService.LoginAsync(model);
                if (result.Success && result.Token != null)
                {
                    HttpContext.Session.SetString("JWTToken", result.Token);
                    HttpContext.Session.SetString("UserId", result.User!.Id);
                }

                // Log the action
                _logger.LogInformation("User {Email} logged in successfully", model.Email);

                TempData["SuccessMessage"] = "Login successful!";

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", model.Email);
                TempData["ErrorMessage"] = "An error occurred during login. Please try again.";
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name;

            // Sign out from cookie authentication
            await _signInManager.SignOutAsync();

            // Call auth service logout
            if (!string.IsNullOrEmpty(userId))
            {
                await _authService.LogoutAsync(userId);
            }

            // Clear session
            HttpContext.Session.Clear();

            _logger.LogInformation("User {Email} logged out", userEmail);

            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Authorize]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
