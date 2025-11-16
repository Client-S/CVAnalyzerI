using CVAnalyzer.Application.Models;
using CVAnalyzer.Application.Services;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Services
{
    public class CVParserService : ICVParserService
    {
        private readonly ILogger<CVParserService> _logger;

        public CVParserService(ILogger<CVParserService> logger)
        {
            _logger = logger;
        }

        public async Task<ExtractedCVData> ParseAsync(Stream fileStream, string fileName, string fileType)
        {
            try
            {
                return fileType.ToLower() switch
                {
                    ".pdf" => await ParsePdfAsync(fileStream, fileName),
                    ".docx" => await ParseWordAsync(fileStream, fileName),
                    _ => throw new NotSupportedException($"File type {fileType} is not supported")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing CV: {FileName}", fileName);
                throw;
            }
        }

        public async Task<ExtractedCVData> ParsePdfAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Normalize input stream: ensure iText reads from the start and we don't depend on caller's stream position.
                    Stream pdfStream = fileStream;
                    if (fileStream == null)
                        throw new ArgumentNullException(nameof(fileStream));

                    if (!fileStream.CanSeek || fileStream.Position != 0)
                    {
                        var ms = new MemoryStream();
                        try
                        {
                            if (fileStream.CanSeek)
                                fileStream.Position = 0;
                        }
                        catch { /* ignore */ }

                        fileStream.CopyTo(ms);
                        ms.Position = 0;
                        pdfStream = ms;
                    }

                    using var pdfReader = new PdfReader(pdfStream);
                    using var pdfDocument = new PdfDocument(pdfReader);

                    var text = new StringBuilder();

                    var pageCount = pdfDocument.GetNumberOfPages();
                    for (int i = 1; i <= pageCount; i++)
                    {
                        var page = pdfDocument.GetPage(i);
                        var strategy = new LocationTextExtractionStrategy();

                        string pageText = string.Empty;
                        try
                        {
                            // PdfTextExtractor may throw or return null on malformed pages/resources.
                            pageText = PdfTextExtractor.GetTextFromPage(page, strategy) ?? string.Empty;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to extract text from page {Page} of {FileName}. Skipping page.", i, fileName);

                            // Best-effort diagnostic: log page dictionary (may be helpful to debug malformed PDFs)
                            try
                            {
                                var dict = page.GetPdfObject();
                                _logger.LogDebug("Page {Page} PdfDictionary: {Dict}", i, dict);
                            }
                            catch { /* ignore diagnostic failures */ }

                            continue;
                        }

                        if (!string.IsNullOrWhiteSpace(pageText))
                            text.AppendLine(pageText);
                    }

                    var rawText = text.ToString();
                    return ExtractDataFromText(rawText, fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing PDF: {FileName}", fileName);
                    throw;
                }
            });
        }

        public async Task<ExtractedCVData> ParseWordAsync(Stream fileStream, string fileName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var wordDocument = WordprocessingDocument.Open(fileStream, false);
                    var body = wordDocument.MainDocumentPart?.Document.Body;

                    if (body == null)
                        throw new InvalidOperationException("Document body is empty");

                    var text = new StringBuilder();
                    foreach (var paragraph in body.Descendants<Paragraph>())
                    {
                        text.AppendLine(paragraph.InnerText);
                    }

                    var rawText = text.ToString();
                    return ExtractDataFromText(rawText, fileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing Word document: {FileName}", fileName);
                    throw;
                }
            });
        }

        private ExtractedCVData ExtractDataFromText(string text, string fileName)
        {
            var data = new ExtractedCVData
            {
                RawText = text ?? string.Empty
            };

            // Extract Student ID
            data.StudentId = ExtractStudentId(text);

            // Extract Name
            data.Name = ExtractName(text);

            // Extract Email
            data.Email = ExtractEmail(text);

            // Extract Phone
            data.Phone = ExtractPhone(text);

            // Extract Skills
            data.Skills = ExtractSkills(text);

            // Extract Experiences
            data.Experiences = ExtractExperiences(text);

            return data;
        }

        private string ExtractStudentId(string text)
        {
            var patterns = new[]
            {
                @"Student\s*ID\s*[:\-]?\s*([A-Z0-9\-]+)",
                @"Roll\s*No\.?\s*[:\-]?\s*([A-Z0-9\-]+)",
                @"ID\s*[:\-]?\s*([A-Z0-9]{6,})",
                @"Enrollment\s*No\.?\s*[:\-]?\s*([A-Z0-9\-]+)"
            };

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
            }

            return string.Empty;
        }

        private string ExtractName(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines.Take(10))
            {
                var cleanLine = line.Trim();

                if (cleanLine.ToLower().Contains("curriculum vitae") ||
                    cleanLine.ToLower().Contains("resume") ||
                    cleanLine.Length < 3)
                    continue;

                var words = cleanLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length >= 2 && words.Length <= 4 &&
                    words.All(w => !string.IsNullOrEmpty(w) && char.IsUpper(w[0])))
                {
                    return cleanLine;
                }
            }

            return string.Empty;
        }

        private string ExtractEmail(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var emailPattern = @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b";
            var match = Regex.Match(text, emailPattern);
            return match.Success ? match.Value : string.Empty;
        }

        private string ExtractPhone(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var phonePatterns = new[]
            {
                @"\+?[\d\s\-\(\)]{10,}",
                @"\b\d{3}[-.\s]?\d{3}[-.\s]?\d{4}\b",
                @"\b\d{10}\b"
            };

            foreach (var pattern in phonePatterns)
            {
                var match = Regex.Match(text, pattern);
                if (match.Success)
                {
                    return Regex.Replace(match.Value, @"[^\d+]", "");
                }
            }

            return string.Empty;
        }

        private List<string> ExtractSkills(string text)
        {
            var skills = new List<string>();
            if (string.IsNullOrEmpty(text))
                return skills;

            var skillSectionPatterns = new[]
            {
                @"SKILLS?[\s\n:]+(.+?)(?=\n[A-Z]{3,}|\n\n|$)",
                @"TECHNICAL\s+SKILLS?[\s\n:]+(.+?)(?=\n[A-Z]{3,}|\n\n|$)",
                @"CORE\s+COMPETENCIES[\s\n:]+(.+?)(?=\n[A-Z]{3,}|\n\n|$)"
            };

            string skillSection = string.Empty;
            foreach (var pattern in skillSectionPatterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success && match.Groups.Count > 1)
                {
                    skillSection = match.Groups[1].Value;
                    break;
                }
            }

            if (string.IsNullOrEmpty(skillSection))
                skillSection = text;

            var knownSkills = new[]
            {
                "C#", "Python", "Java", "JavaScript", "TypeScript", "C++", "Ruby", "PHP", "Swift", "Kotlin",
                "HTML", "CSS", "React", "Angular", "Vue", "Node.js", "Express", "Django", "Flask", "Spring",
                "ASP.NET", ".NET Core", "Entity Framework", "SQL", "MySQL", "PostgreSQL", "MongoDB", "Redis",
                "Docker", "Kubernetes", "AWS", "Azure", "GCP", "Git", "Jenkins", "CI/CD", "Agile", "Scrum",
                "Machine Learning", "AI", "Data Science", "TensorFlow", "PyTorch", "Pandas", "NumPy",
                "REST API", "GraphQL", "Microservices", "Clean Architecture", "Design Patterns"
            };

            foreach (var skill in knownSkills)
            {
                if (Regex.IsMatch(skillSection, $@"\b{Regex.Escape(skill)}\b", RegexOptions.IgnoreCase))
                {
                    if (!skills.Contains(skill, StringComparer.OrdinalIgnoreCase))
                    {
                        skills.Add(skill);
                    }
                }
            }

            return skills;
        }

        private List<ExperienceInfo> ExtractExperiences(string text)
        {
            var experiences = new List<ExperienceInfo>();
            if (string.IsNullOrEmpty(text))
                return experiences;

            var expSectionPattern = @"(EXPERIENCE|WORK EXPERIENCE|EMPLOYMENT)[\s\n:]+(.+?)(?=\n[A-Z]{3,}\s*\n|$)";
            var match = Regex.Match(text, expSectionPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!match.Success || match.Groups.Count < 3)
                return experiences;

            var experienceSection = match.Groups[2].Value;

            var expEntries = Regex.Split(experienceSection, @"\n(?=[A-Z][a-z]+.*(?:Inc\.|Ltd\.|Corp\.|Company))", RegexOptions.Multiline);

            foreach (var entry in expEntries)
            {
                if (string.IsNullOrWhiteSpace(entry))
                    continue;

                var lines = entry.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 2)
                    continue;

                var exp = new ExperienceInfo();

                exp.Company = lines[0].Trim();

                if (lines.Length > 1)
                    exp.Position = lines[1].Trim();

                var datePattern = @"(\d{4})\s*-\s*(\d{4}|Present|Current)";
                var dateMatch = Regex.Match(entry, datePattern, RegexOptions.IgnoreCase);

                if (dateMatch.Success)
                {
                    exp.Duration = dateMatch.Value;

                    if (int.TryParse(dateMatch.Groups[1].Value, out int startYear))
                    {
                        exp.StartDate = new DateTime(startYear, 1, 1);
                    }

                    var endValue = dateMatch.Groups[2].Value;
                    if (endValue.Equals("Present", StringComparison.OrdinalIgnoreCase) ||
                        endValue.Equals("Current", StringComparison.OrdinalIgnoreCase))
                    {
                        exp.IsCurrent = true;
                    }
                    else if (int.TryParse(endValue, out int endYear))
                    {
                        exp.EndDate = new DateTime(endYear, 12, 31);
                    }
                }

                if (lines.Length > 2)
                {
                    exp.Description = string.Join(" ", lines.Skip(2)).Trim();
                }

                if (!string.IsNullOrEmpty(exp.Company))
                {
                    experiences.Add(exp);
                }
            }

            return experiences;
        }
    }

}
