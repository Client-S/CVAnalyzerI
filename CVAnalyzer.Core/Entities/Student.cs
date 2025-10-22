using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedDate { get; set; }

        // Foreign keys
        public string UploadedByUserId { get; set; } = string.Empty;

        // Navigation properties
        public virtual ApplicationUser UploadedBy { get; set; } = null!;
        public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
        public virtual ICollection<Experience> Experiences { get; set; } = new List<Experience>();
        public virtual ICollection<CVDocument> CVDocuments { get; set; } = new List<CVDocument>();
        public virtual ICollection<ClusterMember> ClusterMemberships { get; set; } = new List<ClusterMember>();
    }
}
