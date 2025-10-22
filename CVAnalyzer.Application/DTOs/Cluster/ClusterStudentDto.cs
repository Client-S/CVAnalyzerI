using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.Cluster
{
    public class ClusterStudentDto
    {
        public int StudentId { get; set; }
        public string StudentNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public double SimilarityScore { get; set; }
    }
}
