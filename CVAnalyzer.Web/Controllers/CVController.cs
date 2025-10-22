using CVAnalyzer.Application.DTOs.CV;
using CVAnalyzer.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [Authorize]
    public class CVController : Controller
    {
        private readonly ICVProcessingService _cvProcessingService;
        private readonly IStudentService _studentService;
        private readonly ILogger<CVController> _logger;

        public CVController(
            ICVProcessingService cvProcessingService,
            IStudentService studentService,
            ILogger<CVController> logger)
        {
            _cvProcessingService = cvProcessingService;
            _studentService = studentService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View(new CVUploadDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(CVUploadDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _cvProcessingService.ProcessSingleCVAsync(
                    model.CVFile,
                    model.StudentId,
                    model.StudentName,
                    model.Email,
                    model.Phone);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"CV uploaded and processed successfully! Extracted {result.ExtractedSkills.Count} skills and {result.ExtractedExperiences.Count} experiences.";
                    return RedirectToAction("Details", "Student", new { id = result.StudentId });
                }

                TempData["ErrorMessage"] = result.Message;
                if (result.Errors.Any())
                {
                    TempData["ErrorMessage"] += $": {string.Join(", ", result.Errors)}";
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading CV");
                TempData["ErrorMessage"] = "An error occurred while uploading the CV.";
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult BulkUpload()
        {
            return View(new BulkCVUploadDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpload(BulkCVUploadDto model)
        {
            if (!ModelState.IsValid || model.CVFiles == null || !model.CVFiles.Any())
            {
                TempData["ErrorMessage"] = "Please select at least one CV file to upload.";
                return View(model);
            }

            try
            {
                var result = await _cvProcessingService.ProcessBulkCVsAsync(
                    model.CVFiles,
                    model.AutoExtractStudentInfo);

                TempData["SuccessMessage"] = $"Processed {result.TotalFiles} files: {result.SuccessCount} successful, {result.FailureCount} failed. Processing time: {result.ProcessingTime.TotalSeconds:F2} seconds.";

                return View("BulkUploadResult", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk upload");
                TempData["ErrorMessage"] = "An error occurred during bulk upload.";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _cvProcessingService.DeleteCVAsync(id);

                if (result)
                    TempData["SuccessMessage"] = "CV deleted successfully.";
                else
                    TempData["ErrorMessage"] = "CV not found.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CV {CVId}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the CV.";
            }

            return RedirectToAction("Index", "Student");
        }
    }
}
