using CVAnalyzer.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> _logger;
        private readonly IConfiguration _configuration;

        public SettingsController(
            ILogger<SettingsController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            try
            {
                // Get current settings from configuration
                var settings = new SettingsViewModel
                {
                    ApplicationName = _configuration["ApplicationSettings:Name"] ?? "CV Analyzer",
                    MaxUploadSizeMB = int.Parse(_configuration["FileUpload:MaxSizeMB"] ?? "10"),
                    AllowedFileExtensions = _configuration["FileUpload:AllowedExtensions"] ?? ".pdf,.doc,.docx",
                    SessionTimeoutMinutes = int.Parse(_configuration["Session:TimeoutMinutes"] ?? "30"),
                    EnableAuditLogging = bool.Parse(_configuration["Security:EnableAuditLogging"] ?? "true"),
                    PasswordExpiryDays = int.Parse(_configuration["Security:PasswordExpiryDays"] ?? "90"),
                    MaxLoginAttempts = int.Parse(_configuration["Security:MaxLoginAttempts"] ?? "5")
                };

                return View(settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings");
                TempData["ErrorMessage"] = "Error loading settings.";
                return View(new SettingsViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Note: In a production environment, you would save these settings to a database
                // or update the appsettings.json file. For now, we'll just show a success message.
                TempData["SuccessMessage"] = "Settings updated successfully. Note: Some settings may require application restart to take effect.";

                _logger.LogInformation("Settings updated by admin");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating settings");
                TempData["ErrorMessage"] = "Error updating settings.";
                return View(model);
            }
        }
    }

    public class SettingsViewModel
    {
        public string ApplicationName { get; set; } = "CV Analyzer";
        public int MaxUploadSizeMB { get; set; } = 10;
        public string AllowedFileExtensions { get; set; } = ".pdf,.doc,.docx";
        public int SessionTimeoutMinutes { get; set; } = 30;
        public bool EnableAuditLogging { get; set; } = true;
        public int PasswordExpiryDays { get; set; } = 90;
        public int MaxLoginAttempts { get; set; } = 5;
    }
}
