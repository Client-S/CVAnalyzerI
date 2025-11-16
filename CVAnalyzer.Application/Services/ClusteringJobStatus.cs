using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Services
{
    public enum ClusteringJobStatus
    {
        Pending,
        Running,
        Completed,
        Failed
    }

    public record ClusteringJobInfo(Guid JobId, string Algorithm, ClusteringJobStatus Status, string? Message, DateTime CreatedAt, DateTime? CompletedAt);

    public interface IClusteringJobService
    {
        Task<Guid> EnqueueKMeansAsync(int k);
        Task<Guid> EnqueueDBSCANAsync(double epsilon, int minPoints);
        Task<ClusteringJobInfo?> GetJobAsync(Guid jobId);
    }
}