using CVAnalyzer.Application.DTOs.CV;
using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Services
{
    public class CVProcessingService : ICVProcessingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICVParserService _parserService;
        private readonly IFileStorageService _fileStorage;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CVProcessingService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CVProcessingService(
            IUnitOfWork unitOfWork,
            ICVParserService parserService,
            IFileStorageService fileStorage,
            IAuditService auditService,
            IConfiguration configuration,
            ILogger<CVProcessingService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _parserService = parserService;
            _fileStorage = fileStorage;
            _auditService = auditService;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CVProcessingResultDto> ProcessSingleCVAsync(
            IFormFile file,
            string studentId,
            string studentName,
            string? email,
            string? phone)
        {
            var result = new CVProcessingResultDto
            {
                FileName = file.FileName
            };

            try
            {
                // Validate file
                var validation = ValidateFile(file);
                if (!validation.IsValid)
                {
                    result.Success = false;
                    result.Message = validation.ErrorMessage;
                    return result;
                }

                // Find or create student
                var student = await _unitOfWork.Students.GetByStudentIdAsync(studentId);

                if (student == null)
                {
                    // Get current user ID properly
                    var userId = GetCurrentUserId();

                    student = new Student
                    {
                        StudentId = studentId,
                        Name = studentName,
                        Email = email ?? string.Empty,
                        Phone = phone ?? string.Empty,
                        UploadedByUserId = userId,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWork.Students.AddAsync(student);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Save file
                var fileExtension = Path.GetExtension(file.FileName);
                var filePath = await _fileStorage.SaveFileAsync(file, student.StudentId);

                // Create CV document record
                var cvDocument = new CVDocument
                {
                    StudentId = student.Id,
                    FileName = file.FileName,
                    FilePath = filePath,
                    FileSize = file.Length,
                    FileType = fileExtension,
                    ProcessingStatus = "Processing",
                    UploadDate = DateTime.UtcNow
                };

                await _unitOfWork.CVDocuments.AddAsync(cvDocument);
                await _unitOfWork.SaveChangesAsync();

                // Parse CV
                using var stream = file.OpenReadStream();
                var extractedData = await _parserService.ParseAsync(stream, file.FileName, fileExtension);

                // Process extracted skills
                var extractedSkills = new List<string>();
                foreach (var skillName in extractedData.Skills)
                {
                    var normalizedName = skillName.ToUpper().Replace(" ", "");
                    var skill = (await _unitOfWork.Skills.FindAsync(s => s.NormalizedName == normalizedName))
                        .FirstOrDefault();

                    if (skill == null)
                    {
                        skill = new Skill
                        {
                            SkillName = skillName,
                            NormalizedName = normalizedName,
                            Category = "Technical", // Default category
                            CreatedDate = DateTime.UtcNow
                        };
                        await _unitOfWork.Skills.AddAsync(skill);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Add to student skills if not already present
                    var existingStudentSkill = student.StudentSkills
                        .FirstOrDefault(ss => ss.SkillId == skill.Id);

                    if (existingStudentSkill == null)
                    {
                        var studentSkill = new StudentSkill
                        {
                            StudentId = student.Id,
                            SkillId = skill.Id,
                            ExtractedText = skillName,
                            ConfidenceScore = 0.8 // Default confidence
                        };
                        await _unitOfWork.SaveChangesAsync();
                    }

                    extractedSkills.Add(skillName);
                }

                // Process extracted experiences
                var extractedExperiences = new List<ExperienceDto>();
                foreach (var expInfo in extractedData.Experiences)
                {
                    var experience = new Experience
                    {
                        StudentId = student.Id,
                        Company = expInfo.Company,
                        Position = expInfo.Position,
                        Duration = expInfo.Duration,
                        Description = expInfo.Description,
                        StartDate = expInfo.StartDate,
                        EndDate = expInfo.EndDate,
                        IsCurrent = expInfo.IsCurrent
                    };

                    await _unitOfWork.Experiences.AddAsync(experience);

                    extractedExperiences.Add(new ExperienceDto
                    {
                        Company = expInfo.Company,
                        Position = expInfo.Position,
                        Duration = expInfo.Duration,
                        Description = expInfo.Description,
                        StartDate = expInfo.StartDate,
                        EndDate = expInfo.EndDate,
                        IsCurrent = expInfo.IsCurrent
                    });
                }

                // Update CV document status
                cvDocument.ProcessingStatus = "Completed";
                cvDocument.ProcessedDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                // Log audit
                await _auditService.LogAsync("UploadCV", "CVDocument", cvDocument.Id.ToString(),
                    new { StudentId = studentId, FileName = file.FileName });

                result.Success = true;
                result.Message = "CV processed successfully";
                result.StudentId = student.Id;
                result.StudentName = student.Name;
                result.ExtractedSkills = extractedSkills;
                result.ExtractedExperiences = extractedExperiences;

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing CV: {FileName}", file.FileName);
                result.Success = false;
                result.Message = "Error processing CV";
                result.Errors.Add(ex.Message);
                return result;
            }
        }

        public async Task<BulkUploadResultDto> ProcessBulkCVsAsync(List<IFormFile> files, bool autoExtract = true)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new BulkUploadResultDto
            {
                TotalFiles = files.Count
            };

            foreach (var file in files)
            {
                try
                {
                    string studentId, studentName;

                    if (autoExtract)
                    {
                        // Extract student info from filename or CV content
                        using var stream = file.OpenReadStream();
                        var fileExtension = Path.GetExtension(file.FileName);
                        var extractedData = await _parserService.ParseAsync(stream, file.FileName, fileExtension);

                        studentId = extractedData.StudentId;
                        studentName = extractedData.Name;

                        // If extraction failed, use filename
                        if (string.IsNullOrEmpty(studentId))
                        {
                            studentId = Path.GetFileNameWithoutExtension(file.FileName);
                        }
                        if (string.IsNullOrEmpty(studentName))
                        {
                            studentName = Path.GetFileNameWithoutExtension(file.FileName);
                        }
                    }
                    else
                    {
                        // Use filename as student ID
                        studentId = Path.GetFileNameWithoutExtension(file.FileName);
                        studentName = studentId;
                    }

                    var cvResult = await ProcessSingleCVAsync(file, studentId, studentName, null, null);
                    result.Results.Add(cvResult);

                    if (cvResult.Success)
                        result.SuccessCount++;
                    else
                        result.FailureCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing bulk CV: {FileName}", file.FileName);
                    result.FailureCount++;
                    result.Results.Add(new CVProcessingResultDto
                    {
                        Success = false,
                        FileName = file.FileName,
                        Message = "Processing failed",
                        Errors = new List<string> { ex.Message }
                    });
                }
            }

            stopwatch.Stop();
            result.ProcessingTime = stopwatch.Elapsed;

            await _auditService.LogAsync("BulkUploadCV", "CVDocument", "Bulk",
                new { TotalFiles = result.TotalFiles, SuccessCount = result.SuccessCount, FailureCount = result.FailureCount });

            return result;
        }

        public async Task<List<CVDocumentDto>> GetStudentCVsAsync(int studentId)
        {
            var cvDocuments = await _unitOfWork.CVDocuments.FindAsync(cv => cv.StudentId == studentId);

            return cvDocuments.Select(cv => new CVDocumentDto
            {
                Id = cv.Id,
                StudentId = cv.Student.StudentId,
                StudentName = cv.Student.Name,
                FileName = cv.FileName,
                FileType = cv.FileType,
                FileSize = cv.FileSize,
                ProcessingStatus = cv.ProcessingStatus,
                UploadDate = cv.UploadDate,
                ProcessedDate = cv.ProcessedDate,
                ErrorMessage = cv.ErrorMessage
            }).ToList();
        }

        public async Task<bool> DeleteCVAsync(int cvId)
        {
            try
            {
                var cvDocument = await _unitOfWork.CVDocuments.GetByIdAsync(cvId);
                if (cvDocument == null)
                    return false;

                // Delete physical file
                await _fileStorage.DeleteFileAsync(cvDocument.FilePath);

                // Delete database record
                await _unitOfWork.CVDocuments.DeleteAsync(cvDocument);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync("DeleteCV", "CVDocument", cvId.ToString(),
                    new { FileName = cvDocument.FileName });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting CV: {CVId}", cvId);
                return false;
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateFile(IFormFile file)
        {
            var maxFileSize = _configuration.GetValue<long>("FileUpload:MaxFileSize", 10485760); // 10MB default
            var allowedExtensions = _configuration.GetSection("FileUpload:AllowedExtensions").Get<string[]>()
                ?? new[] { ".pdf", ".docx" };

            if (file == null || file.Length == 0)
                return (false, "File is empty");

            if (file.Length > maxFileSize)
                return (false, $"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024}MB");

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                return (false, $"File type {extension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");

            return (true, string.Empty);
        }

        // Helper method to get current user ID
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
