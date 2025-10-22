using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string studentId);
        Task<bool> DeleteFileAsync(string filePath);
        Task<byte[]> GetFileAsync(string filePath);
        string GetFilePath(string fileName, string studentId);
    }
}
