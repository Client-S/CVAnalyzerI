using CVAnalyzer.Application.DTOs.User;
using CVAnalyzer.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class UserController : Controller
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserManagementService userManagementService,
            ILogger<UserController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userManagementService.GetAllUsersAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["ErrorMessage"] = "Error loading users.";
                return View(new List<UserListDto>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = UserRoles.GetAllRoles();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return RedirectToAction("Index");
                }

                ViewBag.Roles = UserRoles.GetAllRoles();

                var updateDto = new UpdateUserDto
                {
                    Id = user.Id,
                    FirstName = user.FullName.Split(' ')[0],
                    LastName = user.FullName.Split(' ').Length > 1 ? user.FullName.Split(' ')[1] : "",
                    Department = user.Department,
                    IsActive = user.IsActive,
                    Roles = user.Roles
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user {UserId}", id);
                TempData["ErrorMessage"] = "Error loading user details.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = UserRoles.GetAllRoles();
                return View(model);
            }

            try
            {
                var result = await _userManagementService.UpdateUserAsync(model);

                if (result)
                {
                    TempData["SuccessMessage"] = "User updated successfully.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Failed to update user.";
                ViewBag.Roles = UserRoles.GetAllRoles();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", model.Id);
                TempData["ErrorMessage"] = "An error occurred while updating user.";
                ViewBag.Roles = UserRoles.GetAllRoles();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            try
            {
                var result = await _userManagementService.DeactivateUserAsync(id);

                if (result)
                    TempData["SuccessMessage"] = "User deactivated successfully.";
                else
                    TempData["ErrorMessage"] = "Failed to deactivate user.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while deactivating user.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            try
            {
                var result = await _userManagementService.ActivateUserAsync(id);

                if (result)
                    TempData["SuccessMessage"] = "User activated successfully.";
                else
                    TempData["ErrorMessage"] = "Failed to activate user.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while activating user.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var result = await _userManagementService.DeleteUserAsync(id);

                if (result)
                    TempData["SuccessMessage"] = "User deleted successfully.";
                else
                    TempData["ErrorMessage"] = "Failed to delete user.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting user.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ResetPassword(string id)
        {
            ViewBag.UserId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["ErrorMessage"] = "Password is required.";
                ViewBag.UserId = userId;
                return View();
            }

            try
            {
                var result = await _userManagementService.ResetPasswordAsync(userId, newPassword);

                if (result)
                {
                    TempData["SuccessMessage"] = "Password reset successfully.";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Failed to reset password.";
                ViewBag.UserId = userId;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                TempData["ErrorMessage"] = "An error occurred while resetting password.";
                ViewBag.UserId = userId;
                return View();
            }
        }
    }

}

