using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.CV
{
    public class CVProcessingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public List<string> ExtractedSkills { get; set; } = new();
        public List<ExperienceDto> ExtractedExperiences { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public string FileName { get; set; } = string.Empty;
    }
}
