using CVAnalyzer.Core.Enums;
using CVAnalyzer.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly ILogger<AuditController> _logger;

        public AuditController(
            IAuditService auditService,
            ILogger<AuditController> logger)
        {
            _auditService = auditService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? action = null, string? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                IEnumerable<CVAnalyzer.Application.DTOs.Audit.AuditLogDto> auditLogs;

                // Apply filters if provided
                if (!string.IsNullOrEmpty(userId))
                {
                    auditLogs = await _auditService.GetAuditLogsByUserIdAsync(userId);
                }
                else if (!string.IsNullOrEmpty(action))
                {
                    auditLogs = await _auditService.GetAuditLogsByActionAsync(action);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    auditLogs = await _auditService.GetAuditLogsByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    auditLogs = await _auditService.GetAllAuditLogsAsync();
                }

                // Pass filter values to view for display
                ViewBag.Action = action;
                ViewBag.UserId = userId;
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;

                return View(auditLogs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading audit logs");
                TempData["ErrorMessage"] = "Error loading audit logs.";
                return View(new List<CVAnalyzer.Application.DTOs.Audit.AuditLogDto>());
            }
        }
    }
}
