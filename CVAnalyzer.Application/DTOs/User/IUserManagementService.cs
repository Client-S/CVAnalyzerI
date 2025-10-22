using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.User
{
    public interface IUserManagementService
    {
        Task<IEnumerable<UserListDto>> GetAllUsersAsync();
        Task<UserListDto?> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(UpdateUserDto updateUserDto);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
        Task<bool> DeleteUserAsync(string userId);
        Task<bool> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
        Task<bool> ResetPasswordAsync(string userId, string newPassword);
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);
        Task<bool> UpdateUserRolesAsync(string userId, List<string> roles);
    }
}
