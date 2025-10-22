using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Entities
{
    public class ClusterMember
    {
        public int Id { get; set; }
        public int ClusterId { get; set; }
        public int StudentId { get; set; }
        public double SimilarityScore { get; set; }
        public string MatchingSkills { get; set; } = string.Empty; // JSON array of matching skills

        // Navigation properties
        public virtual StudentCluster Cluster { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
