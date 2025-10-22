using CVAnalyzer.Application.DTOs.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Services
{
    public interface IClusteringService
    {
        Task<List<ClusterDto>> GetAllClustersAsync();
        Task<ClusterDetailDto?> GetClusterDetailsAsync(int clusterId);
        Task<ClusterDetailDto> CreateSimpleClusterAsync(string clusterName, int numberOfGroups);
        Task<bool> DeleteClusterAsync(int clusterId);
        Task<List<ClusterStudentDto>> FindSimilarStudentsAsync(int studentId, int count = 5);
    }
}
