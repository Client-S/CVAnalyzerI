using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.CV
{
    public class CVDocumentDto
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ProcessingStatus { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
