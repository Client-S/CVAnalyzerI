using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CVAnalyzer.Infrastructure.Services
{
    public class ClusteringBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClusteringBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public ClusteringBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ClusteringBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Clustering Background Service started (scheduler).");

            var intervalMinutes = _configuration.GetValue<int>("Clusters:BackgroundIntervalMinutes", 60);
            var cooldownHours = _configuration.GetValue<int>("Clusters:AutoClusterCooldownHours", 24);
            var defaultK = _configuration.GetValue<int>("Clusters:DefaultNumber", 3);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var autoRecompute = _configuration.GetValue<bool>("Clusters:AutoRecomputeOnUpload", false);

                    if (autoRecompute)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var jobService = scope.ServiceProvider.GetRequiredService<IClusteringJobService>();
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                        // Avoid duplicated auto-clusters: check last auto cluster
                        var recentAutoClusters = (await unitOfWork.Clusters.FindAsync(c => c.ClusterName != null && c.ClusterName.StartsWith("Auto-Cluster-")))
                            .OrderByDescending(c => c.CreatedDate)
                            .ToList();

                        var last = recentAutoClusters.FirstOrDefault();
                        if (last != null && (DateTime.UtcNow - last.CreatedDate).TotalHours < cooldownHours)
                        {
                            _logger.LogInformation("Skipping scheduled auto-cluster; last auto-cluster was at {Created}", last.CreatedDate);
                        }
                        else
                        {
                            _logger.LogInformation("Scheduling auto clustering job (K={K})...", defaultK);
                            await jobService.EnqueueKMeansAsync(defaultK);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in clustering background scheduler");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                }
                catch (TaskCanceledException) { break; }
            }
        }
    }
}