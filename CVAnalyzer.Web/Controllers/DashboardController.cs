using CVAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CVAnalyzer.Web.Controllers
{
    //[Authorize]
    public class DashboardController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IUnitOfWork unitOfWork, ILogger<DashboardController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = new DashboardViewModel
                {
                    TotalStudents = await _unitOfWork.Students.CountAsync(),
                    TotalCVs = await _unitOfWork.CVDocuments.CountAsync(cv => cv.ProcessingStatus == "Completed"),
                    TotalSkills = await _unitOfWork.Skills.CountAsync(),
                    TotalClusters = await _unitOfWork.Clusters.CountAsync()
                };

                return View(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard statistics.";
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = new
                {
                    totalStudents = await _unitOfWork.Students.CountAsync(),
                    cvsProcessed = await _unitOfWork.CVDocuments.CountAsync(cv => cv.ProcessingStatus == "Completed"),
                    skillsIdentified = await _unitOfWork.Skills.CountAsync(),
                    clustersCreated = await _unitOfWork.Clusters.CountAsync()
                };

                return Json(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving statistics");
                return Json(new { error = "Failed to load statistics" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentActivity()
        {
            try
            {
                var recentCVs = await _unitOfWork.CVDocuments
                    .FindAsync(cv => true, include: q => q.Include(c => c.Student));

                var activities = recentCVs
                    .OrderByDescending(cv => cv.UploadDate)
                    .Take(10)
                    .Select(cv => new
                    {
                        action = "CV Uploaded",
                        student = cv.Student.Name ?? "Unknown",
                        date = cv.UploadDate.ToString("MMM dd, yyyy HH:mm"),
                        status = cv.ProcessingStatus
                    });

                return Json(activities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activity");
                return Json(new { error = "Failed to load recent activity" });
            }
        }
    }

    // ViewModel for Dashboard
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalCVs { get; set; }
        public int TotalSkills { get; set; }
        public int TotalClusters { get; set; }
    }
}
