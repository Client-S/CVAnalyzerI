using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string entityType, string entityId, object? details = null);
    }

    public class AuditService : IAuditService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityType, string entityId, object? details = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return;

            var userId = GetCurrentUserId();

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";


            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details != null ? JsonSerializer.Serialize(details) : string.Empty,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow
            };

            await _unitOfWork.AuditLogs.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();
        }
        // Helper method to get current user ID
        private string GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Try to get from claims
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                    return userId;

                // Alternative: try Name claim
                var userName = httpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(userName))
                    return userName;
            }

            return "System";
        }
    }
}
