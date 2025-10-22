using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // "Upload", "Delete", "View", "Export"
        public string EntityType { get; set; } = string.Empty; // "Student", "CV", "Report"
        public string EntityId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty; // JSON with additional info
        public string IpAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
