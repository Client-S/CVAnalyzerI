using CVAnalyzer.Application.DTOs.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.Student
{
    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<ExperienceDto> Experiences { get; set; } = new();
        public int CVCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
