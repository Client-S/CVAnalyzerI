using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CVAnalyzer.Application.DTOs.CV
{
    public class CVUploadDto
    {
        [Required(ErrorMessage = "Please select a CV file")]
        public IFormFile CVFile { get; set; } = null!;

        [Required(ErrorMessage = "Student ID is required")]
        [StringLength(50, ErrorMessage = "Student ID cannot exceed 50 characters")]
        public string StudentId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Student name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string StudentName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(200)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string? Phone { get; set; }
    }
}
