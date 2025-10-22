using CVAnalyzer.Application.DTOs.User;
using CVAnalyzer.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Services
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            IAuditService auditService)
        {
            _userManager = userManager;
            _auditService = auditService;
        }

        public async Task<IEnumerable<UserListDto>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new List<UserListDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new UserListDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Department = user.Department,
                    Roles = roles.ToList(),
                    IsActive = user.IsActive,
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate
                });
            }

            return userList;
        }

        public async Task<UserListDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserListDto
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = $"{user.FirstName} {user.LastName}",
                Department = user.Department,
                Roles = roles.ToList(),
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            };
        }

        public async Task<bool> UpdateUserAsync(UpdateUserDto updateUserDto)
        {
            var user = await _userManager.FindByIdAsync(updateUserDto.Id);
            if (user == null) return false;

            user.FirstName = updateUserDto.FirstName;
            user.LastName = updateUserDto.LastName;
            user.Department = updateUserDto.Department;
            user.IsActive = updateUserDto.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                // Update roles
                await UpdateUserRolesAsync(user.Id, updateUserDto.Roles);
                await _auditService.LogAsync("UpdateUser", "User", user.Id, updateUserDto);
            }

            return result.Succeeded;
        }

        public async Task<bool> DeactivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("DeactivateUser", "User", userId, null);
            }

            return result.Succeeded;
        }

        public async Task<bool> ActivateUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("ActivateUser", "User", userId, null);
            }

            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("DeleteUser", "User", userId, new { Email = user.Email });
            }

            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
        {
            var user = await _userManager.FindByIdAsync(changePasswordDto.UserId);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("ChangePassword", "User", user.Id, null);
            }

            return result.Succeeded;
        }

        public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                await _auditService.LogAsync("ResetPassword", "User", userId, null);
            }

            return result.Succeeded;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<bool> UpdateUserRolesAsync(string userId, List<string> roles)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove old roles
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return false;

            // Add new roles
            var addResult = await _userManager.AddToRolesAsync(user, roles);

            if (addResult.Succeeded)
            {
                await _auditService.LogAsync("UpdateUserRoles", "User", userId, new { Roles = roles });
            }

            return addResult.Succeeded;
        }
    }
}
