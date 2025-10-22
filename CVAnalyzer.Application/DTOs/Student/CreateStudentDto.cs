using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.Student
{
    public class CreateStudentDto
    {
        [Required]
        [StringLength(50)]
        public string StudentId { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }
    }
}
