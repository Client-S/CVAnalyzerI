using CVAnalyzer.Application.Services;
using CVAnalyzer.Infrastructure.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Services
{
    public class ClusteringJobService : IClusteringJobService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IMLClusteringService _mlClusteringService;
        private readonly ILogger<ClusteringJobService> _logger;
        private readonly ConcurrentDictionary<Guid, ClusteringJobInfo> _jobs = new();

        public ClusteringJobService(
            IBackgroundTaskQueue taskQueue,
            IMLClusteringService mlClusteringService,
            ILogger<ClusteringJobService> logger)
        {
            _taskQueue = taskQueue;
            _mlClusteringService = mlClusteringService;
            _logger = logger;
        }

        public Task<ClusteringJobInfo?> GetJobAsync(Guid jobId)
        {
            _jobs.TryGetValue(jobId, out var info);
            return Task.FromResult(info);
        }

        public Task<Guid> EnqueueKMeansAsync(int k)
        {
            var jobId = Guid.NewGuid();
            var info = new ClusteringJobInfo(jobId, $"KMeans(K={k})", ClusteringJobStatus.Pending, null, DateTime.UtcNow, null);
            _jobs[jobId] = info;

            _taskQueue.QueueBackgroundWorkItem(async ct =>
            {
                _jobs[jobId] = info with { Status = ClusteringJobStatus.Running };
                _logger.LogInformation("Started clustering job {JobId} K={K}", jobId, k);
                try
                {
                    await _mlClusteringService.CreateKMeansClusterAsync($"Queued-KMeans-{DateTime.UtcNow:yyyyMMdd-HHmm}-{jobId}", k);
                    _jobs[jobId] = info with { Status = ClusteringJobStatus.Completed, CompletedAt = DateTime.UtcNow, Message = "Completed" };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Clustering job {JobId} failed", jobId);
                    _jobs[jobId] = info with { Status = ClusteringJobStatus.Failed, CompletedAt = DateTime.UtcNow, Message = ex.Message };
                }
            });

            return Task.FromResult(jobId);
        }

        public Task<Guid> EnqueueDBSCANAsync(double epsilon, int minPoints)
        {
            var jobId = Guid.NewGuid();
            var info = new ClusteringJobInfo(jobId, $"DBSCAN(ε={epsilon},minPts={minPoints})", ClusteringJobStatus.Pending, null, DateTime.UtcNow, null);
            _jobs[jobId] = info;

            _taskQueue.QueueBackgroundWorkItem(async ct =>
            {
                _jobs[jobId] = info with { Status = ClusteringJobStatus.Running };
                _logger.LogInformation("Started DBSCAN job {JobId}", jobId);
                try
                {
                    await _mlClusteringService.CreateDBSCANClusterAsync($"Queued-DBSCAN-{DateTime.UtcNow:yyyyMMdd-HHmm}-{jobId}", epsilon, minPoints);
                    _jobs[jobId] = info with { Status = ClusteringJobStatus.Completed, CompletedAt = DateTime.UtcNow, Message = "Completed" };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DBSCAN job {JobId} failed", jobId);
                    _jobs[jobId] = info with { Status = ClusteringJobStatus.Failed, CompletedAt = DateTime.UtcNow, Message = ex.Message };
                }
            });

            return Task.FromResult(jobId);
        }
    }
}