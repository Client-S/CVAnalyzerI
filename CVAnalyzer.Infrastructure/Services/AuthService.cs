using CVAnalyzer.Application.DTOs.Auth;
using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IAuditService auditService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _auditService = auditService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Account is locked. Please try again later."
                    };
                }

                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Invalid email or password"
                };
            }

            // Update last login
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Generate JWT token
            var token = await GenerateJwtTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            await _auditService.LogAsync("Login", "User", user.Id, new { Email = user.Email });

            return new AuthResponseDto
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"])),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = user.Department,
                    Roles = roles.ToList(),
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate
                }
            };
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User with this email already exists"
                };
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Department = registerDto.Department,
                EmailConfirmed = true, // Set to false in production and send confirmation email
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, registerDto.Role);

            await _auditService.LogAsync("Register", "User", user.Id, new { Email = user.Email, Role = registerDto.Role });

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful"
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "User not found or inactive"
                };
            }

            var token = await GenerateJwtTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Token refreshed successfully",
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"])),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Department = user.Department,
                    Roles = roles.ToList(),
                    CreatedDate = user.CreatedDate,
                    LastLoginDate = user.LastLoginDate
                }
            };
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            await _signInManager.SignOutAsync();
            await _auditService.LogAsync("Logout", "User", userId, null);
            return true;
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Department = user.Department,
                Roles = roles.ToList(),
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate
            };
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("FirstName", user.FirstName),
            new Claim("LastName", user.LastName),
            new Claim("Department", user.Department)
        };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:ExpiryMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
