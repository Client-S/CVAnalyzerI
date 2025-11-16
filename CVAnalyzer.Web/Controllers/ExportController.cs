using CVAnalyzer.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CVAnalyzer.Web.Controllers
{
    [Route("[controller]")]
    public class ExportController : Controller
    {
        private readonly IExportService _exportService;
        private readonly ILogger<ExportController> _logger;

        public ExportController(IExportService exportService, ILogger<ExportController> logger)
        {
            _exportService = exportService;
            _logger = logger;
        }

        [HttpGet("Download")]
        public async Task<IActionResult> Download()
        {
            try
            {
                var content = await _exportService.GenerateReportAsync();
                var fileName = $"StudentClusters_{DateTime.UtcNow:yyyyMMddHHmm}.xlsx";
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating export");
                return StatusCode(500, "Error generating export");
            }
        }
    }
}