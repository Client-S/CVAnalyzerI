using CVAnalyzer.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CVAnalyzer.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClusterJobsController : ControllerBase
    {
        private readonly IClusteringJobService _jobService;
        private readonly ILogger<ClusterJobsController> _logger;

        public ClusterJobsController(IClusteringJobService jobService, ILogger<ClusterJobsController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        [HttpPost("kmeans")]
        public async Task<IActionResult> EnqueueKMeans([FromQuery] int k = 3)
        {
            try
            {
                var jobId = await _jobService.EnqueueKMeansAsync(k);
                _logger.LogInformation("Enqueued KMeans clustering job {JobId}", jobId);
                return Accepted(new { JobId = jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enqueuing KMeans job");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("dbscan")]
        public async Task<IActionResult> EnqueueDBSCAN([FromQuery] double eps = 0.5, [FromQuery] int minPts = 2)
        {
            try
            {
                var jobId = await _jobService.EnqueueDBSCANAsync(eps, minPts);
                _logger.LogInformation("Enqueued DBSCAN clustering job {JobId}", jobId);
                return Accepted(new { JobId = jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enqueuing DBSCAN job");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{jobId}")]
        public async Task<IActionResult> GetStatus(Guid jobId)
        {
            try
            {
                var info = await _jobService.GetJobAsync(jobId);
                if (info == null) return NotFound();
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job status");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}