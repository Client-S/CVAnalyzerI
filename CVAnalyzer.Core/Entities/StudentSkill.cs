using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Entities
{
    public class StudentSkill
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SkillId { get; set; }
        public string ProficiencyLevel { get; set; } = string.Empty; // e.g., "Beginner", "Intermediate", "Advanced"
        public string ExtractedText { get; set; } = string.Empty; 
        public double ConfidenceScore { get; set; } // ML confidence

        // Navigation properties
        public virtual Student Student { get; set; } = null!;
        public virtual Skill Skill { get; set; } = null!;
    }

}
