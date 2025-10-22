using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [Authorize(Roles = "Admin,Faculty")]
    public class ReportController : Controller
    {
        private readonly IExportService _exportService;
        private readonly IStudentService _studentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportController> _logger;

        public ReportController(
            IExportService exportService,
            IStudentService studentService,
            CVAnalyzer.Core.Interfaces.IUnitOfWork unitOfWork,
            ILogger<ReportController> logger)
        {
            _exportService = exportService;
            _studentService = studentService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = new
                {
                    TotalStudents = await _unitOfWork.Students.CountAsync(),
                    TotalSkills = await _unitOfWork.Skills.CountAsync(),
                    TotalClusters = await _unitOfWork.Clusters.CountAsync()
                };

                ViewBag.Stats = stats;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading reports page");
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportStudents()
        {
            try
            {
                var fileBytes = await _exportService.ExportStudentsToExcelAsync();
                var fileName = $"Students_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting students");
                TempData["ErrorMessage"] = "Error generating student export.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportCluster(int id)
        {
            try
            {
                var fileBytes = await _exportService.ExportClusterToExcelAsync(id);
                var fileName = $"Cluster_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting cluster {ClusterId}", id);
                TempData["ErrorMessage"] = "Error generating cluster export.";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportSkills()
        {
            try
            {
                var fileBytes = await _exportService.ExportSkillsReportAsync();
                var fileName = $"Skills_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting skills report");
                TempData["ErrorMessage"] = "Error generating skills report.";
                return RedirectToAction("Index");
            }
        }
    }
}
