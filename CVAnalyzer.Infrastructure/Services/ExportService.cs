using ClosedXML.Excel;
using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CVAnalyzer.Infrastructure.Services
{
    public class ExportService : IExportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ExportService> _logger;

        public ExportService(IUnitOfWork unitOfWork, ILogger<ExportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<byte[]> GenerateReportAsync()
        {
            try
            {
                // Load students with all related entities for export

                var students = (await _unitOfWork.Students.FindAsync(

                    s => true,

                    include: q => q

                        .Include(s => s.StudentSkills)

                            .ThenInclude(ss => ss.Skill)

                        .Include(s => s.Experiences)

                        .Include(s => s.CVDocuments))).ToList();



                // Load clusters with all related entities for export

                var clusters = (await _unitOfWork.Clusters.FindAsync(

                    c => true,

                    include: q => q

                        .Include(c => c.Members)

                            .ThenInclude(m => m.Student)

                                .ThenInclude(s => s.StudentSkills)
                                    .ThenInclude(ss => ss.Skill))).ToList();

                var wb = new XLWorkbook();

                // Students worksheet
                var wsStudents = wb.Worksheets.Add("Students");
                CreateStudentsSheet(wsStudents, students);

                // Clusters worksheet
                var wsClusters = wb.Worksheets.Add("Clusters");
                CreateClustersSheet(wsClusters, clusters);

                // Save to memory stream
                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                ms.Position = 0;
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                throw;
            }
        }

        public async Task<byte[]> ExportStudentsToExcelAsync()
        {
            try
            {
                // Load students with all related entities for export

                var students = (await _unitOfWork.Students.FindAsync(

                    s => true,

                    include: q => q

                        .Include(s => s.StudentSkills)

                            .ThenInclude(ss => ss.Skill)

                        .Include(s => s.Experiences)
                        .Include(s => s.CVDocuments))).ToList();

                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Students");

                CreateStudentsSheet(ws, students);

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                ms.Position = 0;
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting students to Excel");
                throw;
            }
        }

        public async Task<byte[]> ExportClusterToExcelAsync(int clusterId)
        {
            try
            {
                // Load cluster with all related entities for export

                var clusterData = await _unitOfWork.Clusters.FindAsync(

                    c => c.Id == clusterId,

                    include: q => q

                        .Include(c => c.Members)

                            .ThenInclude(m => m.Student)

                                .ThenInclude(s => s.StudentSkills)

                                    .ThenInclude(ss => ss.Skill));



                var cluster = clusterData.FirstOrDefault();
                if (cluster == null)
                    throw new ArgumentException($"Cluster with ID {clusterId} not found");

                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Cluster Details");

                // Header information
                ws.Cell(1, 1).Value = "Cluster Information";
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Font.FontSize = 14;

                ws.Cell(2, 1).Value = "Cluster Name";
                ws.Cell(2, 2).Value = cluster.ClusterName;

                ws.Cell(3, 1).Value = "Algorithm";
                ws.Cell(3, 2).Value = cluster.Algorithm;

                ws.Cell(4, 1).Value = "Member Count";
                ws.Cell(4, 2).Value = cluster.MemberCount;

                ws.Cell(5, 1).Value = "Created Date";
                ws.Cell(5, 2).Value = cluster.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");

                // Members section
                ws.Cell(7, 1).Value = "Cluster Members";
                ws.Cell(7, 1).Style.Font.Bold = true;

                var headerRow = 8;
                ws.Cell(headerRow, 1).Value = "Student ID";
                ws.Cell(headerRow, 2).Value = "Student Name";
                ws.Cell(headerRow, 3).Value = "Email";
                ws.Cell(headerRow, 4).Value = "Similarity Score";
                ws.Cell(headerRow, 5).Value = "Matching Skills";

                var headerRange = ws.Range(headerRow, 1, headerRow, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

                if (cluster.Members != null && cluster.Members.Any())
                {
                    for (int i = 0; i < cluster.Members.Count; i++)
                    {
                        var row = headerRow + i + 1;
                        var member = cluster.Members.ElementAt(i);

                        ws.Cell(row, 1).Value = member.Student?.StudentId ?? string.Empty;
                        ws.Cell(row, 2).Value = member.Student?.Name ?? string.Empty;
                        ws.Cell(row, 3).Value = member.Student?.Email ?? string.Empty;
                        ws.Cell(row, 4).Value = member.SimilarityScore;
                        ws.Cell(row, 5).Value = member.MatchingSkills ?? string.Empty;
                    }
                }

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                ms.Position = 0;
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting cluster {ClusterId} to Excel", clusterId);
                throw;
            }
        }

        public async Task<byte[]> ExportSkillsReportAsync()
        {
            try
            {
                // Load students with skills for the report

                var students = (await _unitOfWork.Students.FindAsync(

                    s => true,

                    include: q => q

                        .Include(s => s.StudentSkills)
                            .ThenInclude(ss => ss.Skill))).ToList();

                var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Skills Report");

                // Header
                ws.Cell(1, 1).Value = "Skill";
                ws.Cell(1, 2).Value = "Student Count";
                ws.Cell(1, 3).Value = "Students";

                var headerRange = ws.Range(1, 1, 1, 3);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightYellow;

                // Group skills and count occurrences
                var skillGroups = students
                    .Where(s => s.StudentSkills != null && s.StudentSkills.Any())
                    .SelectMany(s => s.StudentSkills.Select(ss => new { ss.Skill?.SkillName, s.Name, s.StudentId }))
                    .Where(x => !string.IsNullOrEmpty(x.SkillName))
                    .GroupBy(x => x.SkillName)
                    .OrderByDescending(g => g.Count())
                    .ToList();

                for (int i = 0; i < skillGroups.Count; i++)
                {
                    var row = i + 2;
                    var group = skillGroups[i];

                    ws.Cell(row, 1).Value = group.Key;
                    ws.Cell(row, 2).Value = group.Count();
                    ws.Cell(row, 3).Value = string.Join(", ", group.Select(x => $"{x.StudentId} - {x.Name}"));
                }

                ws.Columns().AdjustToContents();

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                ms.Position = 0;
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting skills report");
                throw;
            }
        }

        private void CreateStudentsSheet(IXLWorksheet ws, List<Student> students)
        {
            // Create header row with formatting
            ws.Cell(1, 1).Value = "Student ID";
            ws.Cell(1, 2).Value = "Name";
            ws.Cell(1, 3).Value = "Email";
            ws.Cell(1, 4).Value = "Phone";
            ws.Cell(1, 5).Value = "Skills";
            ws.Cell(1, 6).Value = "Experience Count";

            // Format header
            var headerRange = ws.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;

            // Populate data rows
            for (int i = 0; i < students.Count; i++)
            {
                var row = i + 2;
                var student = students[i];

                ws.Cell(row, 1).Value = student.StudentId ?? string.Empty;
                ws.Cell(row, 2).Value = student.Name ?? string.Empty;
                ws.Cell(row, 3).Value = student.Email ?? string.Empty;
                ws.Cell(row, 4).Value = student.Phone ?? string.Empty;

                // Extract unique skills
                var skills = student.StudentSkills != null && student.StudentSkills.Any()
                    ? string.Join(", ", student.StudentSkills.Select(ss => ss.Skill?.SkillName ?? string.Empty).Where(s => !string.IsNullOrEmpty(s)).Distinct())
                    : string.Empty;
                ws.Cell(row, 5).Value = skills;

                // Experience count
                ws.Cell(row, 6).Value = student.Experiences?.Count ?? 0;
            }

            // Auto-fit columns
            ws.Columns().AdjustToContents();
        }

        private void CreateClustersSheet(IXLWorksheet ws, List<StudentCluster> clusters)
        {
            // Create header row with formatting
            ws.Cell(1, 1).Value = "Cluster Name";
            ws.Cell(1, 2).Value = "Algorithm";
            ws.Cell(1, 3).Value = "Member Count";
            ws.Cell(1, 4).Value = "Members (StudentId - Name)";
            ws.Cell(1, 5).Value = "Common Skills";
            ws.Cell(1, 6).Value = "Created Date";

            // Format header
            var headerRange = ws.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightGreen;

            // Populate data rows
            for (int i = 0; i < clusters.Count; i++)
            {
                var row = i + 2;
                var cluster = clusters[i];

                ws.Cell(row, 1).Value = cluster.ClusterName ?? string.Empty;
                ws.Cell(row, 2).Value = cluster.Algorithm ?? string.Empty;
                ws.Cell(row, 3).Value = cluster.MemberCount;

                // Members list
                var members = cluster.Members != null && cluster.Members.Any()
                    ? string.Join("; ", cluster.Members.Select(m => $"{m.Student?.StudentId ?? "N/A"} - {m.Student?.Name ?? "N/A"}"))
                    : string.Empty;
                ws.Cell(row, 4).Value = members;

                // Common skills (deduplicated and top 5)
                var commonSkills = ExtractCommonSkills(cluster.Members);
                ws.Cell(row, 5).Value = commonSkills;

                // Created date
                ws.Cell(row, 6).Value = cluster.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // Auto-fit columns
            ws.Columns().AdjustToContents();
        }

        private string ExtractCommonSkills(ICollection<ClusterMember> members)
        {
            if (members == null || !members.Any())
                return string.Empty;

            try
            {
                var skills = members
                    .Where(m => !string.IsNullOrEmpty(m.MatchingSkills))
                    .SelectMany(m => m.MatchingSkills.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .GroupBy(s => s)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .Take(5)
                    .ToList();

                return string.Join(", ", skills);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting common skills");
                return string.Empty;
            }
        }
    }
}
