using CVAnalyzer.Application.DTOs.CV;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Services
{
    public interface ICVProcessingService
    {
        Task<CVProcessingResultDto> ProcessSingleCVAsync(IFormFile file, string studentId, string studentName, string? email, string? phone);
        Task<BulkUploadResultDto> ProcessBulkCVsAsync(List<IFormFile> files, bool autoExtract = true);
        Task<List<CVDocumentDto>> GetStudentCVsAsync(int studentId);
        Task<bool> DeleteCVAsync(int cvId);
    }
}
