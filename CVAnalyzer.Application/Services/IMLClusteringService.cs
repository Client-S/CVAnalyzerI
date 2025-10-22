using CVAnalyzer.Application.DTOs.Cluster;

namespace CVAnalyzer.Application.Services
{
    public interface IMLClusteringService
    {
        Task<ClusterDetailDto> CreateKMeansClusterAsync(string clusterName, int numberOfClusters);
        Task<ClusterDetailDto> CreateDBSCANClusterAsync(string clusterName, double epsilon = 0.5, int minPoints = 2);
        Task<List<string>> FindSimilarSkillsAsync(string skill, int topN = 5);
        Task<double> CalculateSemanticSimilarityAsync(string skill1, string skill2);
    }
}
