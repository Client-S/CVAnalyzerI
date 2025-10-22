using CVAnalyzer.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Services
{
    public interface ICVParserService
    {
        Task<ExtractedCVData> ParsePdfAsync(Stream fileStream, string fileName);
        Task<ExtractedCVData> ParseWordAsync(Stream fileStream, string fileName);
        Task<ExtractedCVData> ParseAsync(Stream fileStream, string fileName, string fileType);
    }
}
