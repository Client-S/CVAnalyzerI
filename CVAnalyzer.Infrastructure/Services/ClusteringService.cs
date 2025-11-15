using CVAnalyzer.Application.DTOs.Cluster;
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
    public class ClusteringService : IClusteringService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ClusteringService> _logger;

        public ClusteringService(
            IUnitOfWork unitOfWork,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ClusteringService> logger)
        {
            _unitOfWork = unitOfWork;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<ClusterDto>> GetAllClustersAsync()
        {
            var clusters = await _unitOfWork.Clusters.FindAsync(

                c => true,

                include: q => q.Include(c => c.Members));

            return clusters.Select(c => new ClusterDto
            {
                Id = c.Id,
                ClusterName = c.ClusterName,
                Description = c.Description,
                StudentCount = c.MemberCount,
                CommonSkills = ExtractCommonSkills(c.Members.ToList()),
                CreatedDate = c.CreatedDate
            }).ToList();
        }

        public async Task<ClusterDetailDto?> GetClusterDetailsAsync(int clusterId)
        {
            var clusterMembers = await _unitOfWork.Clusters.FindAsync(

               c => c.Id == clusterId,

               include: q => q

                   .Include(c => c.Members)

                       .ThenInclude(m => m.Student)

                           .ThenInclude(s => s.StudentSkills)

                               .ThenInclude(ss => ss.Skill));



            var cluster = clusterMembers.FirstOrDefault();

            if (cluster == null) return null;



            return new ClusterDetailDto

            {

                Id = cluster.Id,

                ClusterName = cluster.ClusterName,

                Description = cluster.Description,

                CommonSkills = ExtractCommonSkills(cluster.Members.ToList()),

                Students = cluster.Members.Select(m => new ClusterStudentDto

                {

                    StudentId = m.Student.Id,

                    StudentNo = m.Student.StudentId,

                    Name = m.Student.Name,

                    Email = m.Student.Email,

                    Skills = m.Student.StudentSkills.Select(ss => ss.Skill.SkillName).ToList(),

                    SimilarityScore = m.SimilarityScore

                }).ToList(),

                CreatedDate = cluster.CreatedDate

            };
        }

        public async Task<ClusterDetailDto> CreateSimpleClusterAsync(string clusterName, int numberOfGroups)
        {
            try
            {
                // Get all students with their skills
                var students = (await _unitOfWork.Students.GetStudentsWithSkillsAsync()).ToList();

                if (students.Count == 0)
                    throw new InvalidOperationException("No students found to cluster");

                // Simple grouping by skill count similarity
                var groupedStudents = GroupStudentsBySkillCount(students, numberOfGroups);

                // Get current user
                var userId = GetCurrentUserId();

                // Create cluster
                var cluster = new StudentCluster
                {
                    ClusterName = clusterName,
                    Algorithm = "Simple Skill Count",
                    Description = $"Grouped into {numberOfGroups} groups based on skill similarity",
                    MemberCount = students.Count,
                    CreatedByUserId = userId,
                    CreatedDate = DateTime.UtcNow
                };

                await _unitOfWork.Clusters.AddAsync(cluster);
                await _unitOfWork.SaveChangesAsync();

                // Add students to cluster
                foreach (var student in students)
                {
                    var similarityScore = CalculateSimpleScore(student);
                    var matchingSkills = student.StudentSkills.Select(ss => ss.Skill.SkillName).ToList();

                    var member = new ClusterMember
                    {
                        ClusterId = cluster.Id,
                        StudentId = student.Id,
                        SimilarityScore = similarityScore,
                        MatchingSkills = string.Join(",", matchingSkills)
                    };
                    cluster.Members.Add(member);
                   
                }
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync("CreateCluster", "Cluster", cluster.Id.ToString(),
                    new { ClusterName = clusterName, StudentCount = students.Count });

                return await GetClusterDetailsAsync(cluster.Id)
                    ?? throw new InvalidOperationException("Failed to retrieve cluster details");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cluster");
                throw;
            }
        }

        public async Task<bool> DeleteClusterAsync(int clusterId)
        {
            try
            {
                var cluster = await _unitOfWork.Clusters.GetByIdAsync(clusterId);
                if (cluster == null) return false;

                await _unitOfWork.Clusters.DeleteAsync(cluster);
                await _unitOfWork.SaveChangesAsync();

                await _auditService.LogAsync("DeleteCluster", "Cluster", clusterId.ToString(), null);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting cluster {ClusterId}", clusterId);
                return false;
            }
        }

        public async Task<List<ClusterStudentDto>> FindSimilarStudentsAsync(int studentId, int count = 5)
        {
            // Use GetByIdWithDetailsAsync to load StudentSkills with Skill entities

            var targetStudent = await _unitOfWork.Students.GetByIdWithDetailsAsync(studentId);

            if (targetStudent == null) return new List<ClusterStudentDto>();

            var allStudents = (await _unitOfWork.Students.GetStudentsWithSkillsAsync())
                .Where(s => s.Id != studentId)
                .ToList();

            var targetSkills = targetStudent.StudentSkills.Select(ss => ss.Skill.SkillName).ToHashSet();

            // Calculate similarity and get top matches
            var similarStudents = allStudents
                .Select(s => new
                {
                    Student = s,
                    Score = CalculateSimilarity(targetSkills,
                        s.StudentSkills.Select(ss => ss.Skill.SkillName).ToHashSet())
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Take(count)
                .Select(x => new ClusterStudentDto
                {
                    StudentId = x.Student.Id,
                    StudentNo = x.Student.StudentId,
                    Name = x.Student.Name,
                    Email = x.Student.Email,
                    Skills = x.Student.StudentSkills.Select(ss => ss.Skill.SkillName).ToList(),
                    SimilarityScore = x.Score
                })
                .ToList();

            return similarStudents;
        }

        // Helper: Simple similarity calculation
        private double CalculateSimilarity(HashSet<string> skills1, HashSet<string> skills2)
        {
            if (skills1.Count == 0 || skills2.Count == 0) return 0;

            var commonSkills = skills1.Intersect(skills2).Count();
            var totalSkills = skills1.Union(skills2).Count();

            return (double)commonSkills / totalSkills * 100;
        }

        // Helper: Simple score calculation
        private double CalculateSimpleScore(Student student)
        {
            var skillCount = student.StudentSkills.Count;
            var expCount = student.Experiences.Count;

            return (skillCount * 0.7) + (expCount * 0.3);
        }

        // Helper: Group students by skill count
        private List<List<Student>> GroupStudentsBySkillCount(List<Student> students, int numberOfGroups)
        {
            var sorted = students.OrderByDescending(s => s.StudentSkills.Count).ToList();
            var groups = new List<List<Student>>();

            var studentsPerGroup = (int)Math.Ceiling((double)students.Count / numberOfGroups);

            for (int i = 0; i < numberOfGroups; i++)
            {
                groups.Add(sorted.Skip(i * studentsPerGroup).Take(studentsPerGroup).ToList());
            }

            return groups;
        }

        // Helper: Extract common skills
        private List<string> ExtractCommonSkills(List<ClusterMember> members)
        {
            if (members.Count == 0) return new List<string>();

            var allSkills = members
                .SelectMany(m => m.MatchingSkills.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .GroupBy(s => s.Trim())
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToList();

            return allSkills;
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
