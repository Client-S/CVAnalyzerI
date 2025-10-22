using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Entities
{
    public class Skill
    {
        public int Id { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string NormalizedName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<StudentSkill> StudentSkills { get; set; } = new List<StudentSkill>();
    }
}
