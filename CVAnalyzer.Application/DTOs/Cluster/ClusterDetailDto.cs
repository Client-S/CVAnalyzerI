using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.Cluster
{
    public class ClusterDetailDto
    {
        public int Id { get; set; }
        public string ClusterName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> CommonSkills { get; set; } = new();
        public List<ClusterStudentDto> Students { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }
}
