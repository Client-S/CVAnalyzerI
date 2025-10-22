using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Models
{
    public class SkillMapping
    {
        public string OriginalSkill { get; set; } = string.Empty;
        public List<string> Synonyms { get; set; } = new();
        public string NormalizedSkill { get; set; } = string.Empty;
    }

    public class StudentFeatures
    {
        public string StudentId { get; set; } = string.Empty;
        public float[] SkillVector { get; set; } = Array.Empty<float>();
        public int SkillCount { get; set; }
        public int ExperienceCount { get; set; }
    }
}
