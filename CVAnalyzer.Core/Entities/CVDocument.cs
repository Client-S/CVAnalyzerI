using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Entities
{
    public class CVDocument
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string BlobPath { get; set; } = string.Empty; // For Azure Blob Storage
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty; // "PDF" or "DOCX"
        public string ProcessingStatus { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        public string? ErrorMessage { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedDate { get; set; }

        // Navigation properties
        public virtual Student Student { get; set; } = null!;
    }
}
