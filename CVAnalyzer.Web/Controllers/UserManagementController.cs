using CVAnalyzer.Application.DTOs.User;
using CVAnalyzer.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = UserRoles.Admin)]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            IUserManagementService userManagementService,
            ILogger<UserManagementController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userManagementService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, "An error occurred while retrieving users");
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            try
            {
                var user = await _userManagementService.GetUserByIdAsync(userId);

                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving user");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto updateUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userManagementService.UpdateUserAsync(updateUserDto);

                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", updateUserDto.Id);
                return StatusCode(500, "An error occurred while updating user");
            }
        }

        [HttpPost("{userId}/deactivate")]
        public async Task<IActionResult> DeactivateUser(string userId)
        {
            try
            {
                var result = await _userManagementService.DeactivateUserAsync(userId);

                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating user {UserId}", userId);
                return StatusCode(500, "An error occurred while deactivating user");
            }
        }

        [HttpPost("{userId}/activate")]
        public async Task<IActionResult> ActivateUser(string userId)
        {
            try
            {
                var result = await _userManagementService.ActivateUserAsync(userId);

                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating user {UserId}", userId);
                return StatusCode(500, "An error occurred while activating user");
            }
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var result = await _userManagementService.DeleteUserAsync(userId);

                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return StatusCode(500, "An error occurred while deleting user");
            }
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _userManagementService.ChangePasswordAsync(changePasswordDto);

                if (!result)
                    return BadRequest(new { message = "Failed to change password. Please check current password." });

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", changePasswordDto.UserId);
                return StatusCode(500, "An error occurred while changing password");
            }
        }

        [HttpPost("{userId}/reset-password")]
        public async Task<IActionResult> ResetPassword(string userId, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return BadRequest(new { message = "New password is required" });

            try
            {
                var result = await _userManagementService.ResetPasswordAsync(userId, newPassword);

                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                return StatusCode(500, "An error occurred while resetting password");
            }
        }

        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _userManagementService.GetUserRolesAsync(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
                return StatusCode(500, "An error occurred while retrieving user roles");
            }
        }

        [HttpPut("{userId}/roles")]
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] List<string> roles)
        {
            try
            {
                var result = await _userManagementService.UpdateUserRolesAsync(userId, roles);

                if (!result)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User roles updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for user {UserId}", userId);
                return StatusCode(500, "An error occurred while updating user roles");
            }
        }

        [HttpGet("roles")]
        public IActionResult GetAllRoles()
        {
            var roles = UserRoles.GetAllRoles();
            return Ok(roles);
        }
    }
}
