using CVAnalyzer.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CVAnalyzer.Web.Pages.Export
{
    public class IndexModel : PageModel
    {
        private readonly IExportService _exportService;
        private readonly IClusteringJobService _clusteringJobService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IExportService exportService,
            IClusteringJobService clusteringJobService,
            ILogger<IndexModel> logger)
        {
            _exportService = exportService;
            _clusteringJobService = clusteringJobService;
            _logger = logger;
        }

        public void OnGet()
        {
            // Page load handler
        }

        public async Task<IActionResult> OnPostDownloadExcelAsync()
        {
            try
            {
                var content = await _exportService.GenerateReportAsync();
                var fileName = $"StudentClusters_{DateTime.UtcNow:yyyyMMddHHmm}.xlsx";
                _logger.LogInformation("Generated Excel report: {FileName}", fileName);
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating export");
                TempData["ErrorMessage"] = "Error generating export: " + ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostEnqueueKMeansAsync([FromForm] int k = 3)
        {
            try
            {
                if (k < 2 || k > 10)
                {
                    TempData["ErrorMessage"] = "K must be between 2 and 10";
                    return RedirectToPage();
                }

                var jobId = await _clusteringJobService.EnqueueKMeansAsync(k);
                TempData["SuccessMessage"] = $"K-Means clustering job enqueued. Job ID: {jobId}";
                TempData["JobId"] = jobId.ToString();
                _logger.LogInformation("Enqueued K-Means job {JobId} with K={K}", jobId, k);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enqueuing K-Means clustering");
                TempData["ErrorMessage"] = "Error enqueuing clustering job: " + ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostEnqueueDBSCANAsync([FromForm] double epsilon = 0.5, [FromForm] int minPoints = 2)
        {
            try
            {
                if (epsilon <= 0 || epsilon > 2.0)
                {
                    TempData["ErrorMessage"] = "Epsilon must be between 0 and 2.0";
                    return RedirectToPage();
                }

                if (minPoints < 2 || minPoints > 10)
                {
                    TempData["ErrorMessage"] = "Min Points must be between 2 and 10";
                    return RedirectToPage();
                }

                var jobId = await _clusteringJobService.EnqueueDBSCANAsync(epsilon, minPoints);
                TempData["SuccessMessage"] = $"DBSCAN clustering job enqueued. Job ID: {jobId}";
                TempData["JobId"] = jobId.ToString();
                _logger.LogInformation("Enqueued DBSCAN job {JobId} with eps={Epsilon}, minPts={MinPoints}", jobId, epsilon, minPoints);
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enqueuing DBSCAN clustering");
                TempData["ErrorMessage"] = "Error enqueuing clustering job: " + ex.Message;
                return RedirectToPage();
            }
        }
    }
}