using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Entities
{
    public class StudentCluster
    {
        public int Id { get; set; }
        public string ClusterName { get; set; } = string.Empty;
        public string Algorithm { get; set; } = string.Empty; // "KMeans", "DBSCAN", etc.
        public string Description { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedByUserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser CreatedBy { get; set; } = null!;
        public virtual ICollection<ClusterMember> Members { get; set; } = new List<ClusterMember>();
    }
}
