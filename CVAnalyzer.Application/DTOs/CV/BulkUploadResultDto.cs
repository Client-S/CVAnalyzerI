using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.CV
{
    public class BulkUploadResultDto
    {
        public int TotalFiles { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<CVProcessingResultDto> Results { get; set; } = new();
        public TimeSpan ProcessingTime { get; set; }
    }
}
