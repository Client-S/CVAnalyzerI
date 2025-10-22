using CVAnalyzer.Application.Services;
using CVAnalyzer.Core.Interfaces;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<byte[]> ExportStudentsToExcelAsync()
        {
            try
            {
                var students = (await _unitOfWork.Students.GetStudentsWithSkillsAsync()).ToList();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Students");

                // Headers
                worksheet.Cells[1, 1].Value = "Student ID";
                worksheet.Cells[1, 2].Value = "Name";
                worksheet.Cells[1, 3].Value = "Email";
                worksheet.Cells[1, 4].Value = "Phone";
                worksheet.Cells[1, 5].Value = "Skills";
                worksheet.Cells[1, 6].Value = "Experience Count";
                worksheet.Cells[1, 7].Value = "CV Count";
                worksheet.Cells[1, 8].Value = "Created Date";

                // Style headers
                using (var range = worksheet.Cells[1, 1, 1, 8])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data
                int row = 2;
                foreach (var student in students)
                {
                    worksheet.Cells[row, 1].Value = student.StudentId;
                    worksheet.Cells[row, 2].Value = student.Name;
                    worksheet.Cells[row, 3].Value = student.Email;
                    worksheet.Cells[row, 4].Value = student.Phone;
                    worksheet.Cells[row, 5].Value = string.Join(", ",
                        student.StudentSkills.Select(ss => ss.Skill.SkillName));
                    worksheet.Cells[row, 6].Value = student.Experiences.Count;
                    worksheet.Cells[row, 7].Value = student.CVDocuments.Count;
                    worksheet.Cells[row, 8].Value = student.CreatedDate.ToString("yyyy-MM-dd");
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Add summary
                row += 2;
                worksheet.Cells[row, 1].Value = "Total Students:";
                worksheet.Cells[row, 2].Value = students.Count;
                worksheet.Cells[row, 1, row, 2].Style.Font.Bold = true;

                return package.GetAsByteArray();
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
                var cluster = await _unitOfWork.Clusters.GetByIdAsync(clusterId);
                if (cluster == null)
                    throw new InvalidOperationException("Cluster not found");

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add(cluster.ClusterName);

                // Title
                worksheet.Cells[1, 1].Value = $"Cluster: {cluster.ClusterName}";
                worksheet.Cells[1, 1, 1, 6].Merge = true;
                worksheet.Cells[1, 1].Style.Font.Size = 16;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                worksheet.Cells[2, 1].Value = $"Created: {cluster.CreatedDate:yyyy-MM-dd}";
                worksheet.Cells[2, 1, 2, 6].Merge = true;
                worksheet.Cells[2, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                // Headers
                int headerRow = 4;
                worksheet.Cells[headerRow, 1].Value = "Student ID";
                worksheet.Cells[headerRow, 2].Value = "Name";
                worksheet.Cells[headerRow, 3].Value = "Email";
                worksheet.Cells[headerRow, 4].Value = "Skills";
                worksheet.Cells[headerRow, 5].Value = "Similarity Score";
                worksheet.Cells[headerRow, 6].Value = "Matching Skills";

                // Style headers
                using (var range = worksheet.Cells[headerRow, 1, headerRow, 6])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGreen);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // Data
                int row = headerRow + 1;
                foreach (var member in cluster.Members)
                {
                    worksheet.Cells[row, 1].Value = member.Student.StudentId;
                    worksheet.Cells[row, 2].Value = member.Student.Name;
                    worksheet.Cells[row, 3].Value = member.Student.Email;
                    worksheet.Cells[row, 4].Value = member.Student.StudentSkills.Count;
                    worksheet.Cells[row, 5].Value = member.SimilarityScore.ToString("F2");
                    worksheet.Cells[row, 6].Value = member.MatchingSkills;
                    row++;
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting cluster to Excel");
                throw;
            }
        }

        public async Task<byte[]> ExportSkillsReportAsync()
        {
            try
            {
                var skills = (await _unitOfWork.Skills.GetAllAsync()).ToList();
                var students = (await _unitOfWork.Students.GetStudentsWithSkillsAsync()).ToList();

                using var package = new ExcelPackage();

                // Sheet 1: Skills Summary
                var skillsSheet = package.Workbook.Worksheets.Add("Skills Summary");

                skillsSheet.Cells[1, 1].Value = "Skill Name";
                skillsSheet.Cells[1, 2].Value = "Category";
                skillsSheet.Cells[1, 3].Value = "Student Count";
                skillsSheet.Cells[1, 4].Value = "Percentage";

                using (var range = skillsSheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightCoral);
                }

                int row = 2;
                foreach (var skill in skills.OrderByDescending(s =>
                    students.Count(st => st.StudentSkills.Any(ss => ss.SkillId == s.Id))))
                {
                    var studentCount = students.Count(st => st.StudentSkills.Any(ss => ss.SkillId == skill.Id));
                    var percentage = students.Count > 0 ? (double)studentCount / students.Count * 100 : 0;

                    skillsSheet.Cells[row, 1].Value = skill.SkillName;
                    skillsSheet.Cells[row, 2].Value = skill.Category;
                    skillsSheet.Cells[row, 3].Value = studentCount;
                    skillsSheet.Cells[row, 4].Value = $"{percentage:F1}%";
                    row++;
                }

                skillsSheet.Cells.AutoFitColumns();

                // Sheet 2: Top Skills
                var topSkillsSheet = package.Workbook.Worksheets.Add("Top 10 Skills");

                topSkillsSheet.Cells[1, 1].Value = "Rank";
                topSkillsSheet.Cells[1, 2].Value = "Skill";
                topSkillsSheet.Cells[1, 3].Value = "Students";

                using (var range = topSkillsSheet.Cells[1, 1, 1, 3])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Gold);
                }

                var topSkills = skills
                    .Select(s => new
                    {
                        Skill = s,
                        Count = students.Count(st => st.StudentSkills.Any(ss => ss.SkillId == s.Id))
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToList();

                row = 2;
                int rank = 1;
                foreach (var item in topSkills)
                {
                    topSkillsSheet.Cells[row, 1].Value = rank++;
                    topSkillsSheet.Cells[row, 2].Value = item.Skill.SkillName;
                    topSkillsSheet.Cells[row, 3].Value = item.Count;
                    row++;
                }

                topSkillsSheet.Cells.AutoFitColumns();

                return package.GetAsByteArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting skills report");
                throw;
            }
        }
    }
}
