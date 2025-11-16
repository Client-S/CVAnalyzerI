using CVAnalyzer.Application.DTOs.CV;
using CVAnalyzer.Application.DTOs.Student;
using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly ILogger<StudentService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StudentService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            ILogger<StudentService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
        {
            var students = await _unitOfWork.Students.GetStudentsWithSkillsAsync();

            return students.Select(s => MapToDto(s)).ToList();
        }

        public async Task<StudentDto?> GetStudentByIdAsync(int id)
        {
            var student = await _unitOfWork.Students.GetByIdWithDetailsAsync(id);

            return student == null ? null : MapToDto(student);
        }

        public async Task<StudentDto?> GetStudentByStudentIdAsync(string studentId)
        {
            var student = await _unitOfWork.Students.GetByStudentIdAsync(studentId);
            return student == null ? null : MapToDto(student);
        }

        public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createDto)
        {
            var existingStudent = await _unitOfWork.Students.GetByStudentIdAsync(createDto.StudentId);
            if (existingStudent != null)
                throw new InvalidOperationException($"Student with ID {createDto.StudentId} already exists");

            var userId = GetCurrentUserId();

            var student = new Student
            {
                StudentId = createDto.StudentId,
                Name = createDto.Name,
                Email = createDto.Email ?? string.Empty,
                Phone = createDto.Phone ?? string.Empty,
                UploadedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Students.AddAsync(student);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync("CreateStudent", "Student", student.Id.ToString(), createDto);

            return MapToDto(student);
        }

        public async Task<bool> UpdateStudentAsync(UpdateStudentDto updateDto)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(updateDto.Id);
            if (student == null)
                return false;

            student.Name = updateDto.Name;
            student.Email = updateDto.Email ?? string.Empty;
            student.Phone = updateDto.Phone ?? string.Empty;
            student.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.Students.UpdateAsync(student);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync("UpdateStudent", "Student", student.Id.ToString(), updateDto);

            return true;
        }

        public async Task<bool> DeleteStudentAsync(int id)
        {
            var student = await _unitOfWork.Students.GetByIdAsync(id);
            if (student == null)
                return false;

            await _unitOfWork.Students.DeleteAsync(student);
            await _unitOfWork.SaveChangesAsync();

            await _auditService.LogAsync("DeleteStudent", "Student", id.ToString(),
                new { StudentId = student.StudentId, Name = student.Name });

            return true;
        }

        public async Task<IEnumerable<StudentDto>> SearchStudentsAsync(string searchTerm)
        {
            // Load students with all related entities to prevent N+1 queries

            var students = await _unitOfWork.Students.FindAsync(

                s => s.StudentId.Contains(searchTerm) ||

                     s.Name.Contains(searchTerm) ||

                     s.Email.Contains(searchTerm),

                include: q => q

                    .Include(s => s.StudentSkills)

                        .ThenInclude(ss => ss.Skill)

                    .Include(s => s.Experiences)

                    .Include(s => s.CVDocuments));

            return students.Select(s => MapToDto(s)).ToList();
        }

        public async Task<IEnumerable<StudentDto>> GetStudentsBySkillAsync(string skill)
        {
            var students = await _unitOfWork.Students.SearchBySkillAsync(skill);
            return students.Select(s => MapToDto(s)).ToList();
        }

        private StudentDto MapToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                StudentId = student.StudentId,
                Name = student.Name,
                Email = student.Email,
                Phone = student.Phone,
                Skills = student.StudentSkills.Select(ss => ss.Skill.SkillName).ToList(),
                Experiences = student.Experiences.Select(e => new ExperienceDto
                {
                    Company = e.Company,
                    Position = e.Position,
                    Duration = e.Duration,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsCurrent = e.IsCurrent
                }).ToList(),
                CVDocuments = student.CVDocuments.Select(cv => new CVDocumentDto

                {

                    Id = cv.Id,

                    StudentId = student.StudentId,

                    StudentName = student.Name,

                    FileName = cv.FileName,

                    FileType = cv.FileType,

                    FileSize = cv.FileSize,

                    ProcessingStatus = cv.ProcessingStatus,

                    UploadDate = cv.UploadDate,

                    ProcessedDate = cv.ProcessedDate,

                    ErrorMessage = cv.ErrorMessage

                }).ToList(),
                CVCount = student.CVDocuments.Count,
                CreatedDate = student.CreatedDate
            };
        }
        private string GetCurrentUserId()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // Try to get from claims
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                    return userId;

                // Alternative: try Name claim
                var userName = httpContext.User.Identity.Name;
                if (!string.IsNullOrEmpty(userName))
                    return userName;
            }

            return "System";
        }
    }
}
