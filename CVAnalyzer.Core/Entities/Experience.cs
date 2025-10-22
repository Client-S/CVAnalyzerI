using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Entities
{
    public class Experience
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string Company { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Duration { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public bool IsCurrent { get; set; }

        // Navigation properties
        public virtual Student Student { get; set; } = null!;
    }
}
