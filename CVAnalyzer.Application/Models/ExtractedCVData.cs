using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Models
{
    public class ExtractedCVData
    {
        public string StudentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<ExperienceInfo> Experiences { get; set; } = new();
        public string RawText { get; set; } = string.Empty;
    }
}
