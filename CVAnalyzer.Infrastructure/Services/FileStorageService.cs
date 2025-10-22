using CVAnalyzer.Application.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CVAnalyzer.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _uploadPath;

        public FileStorageService(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;

            _uploadPath = configuration.GetValue<string>("FileUpload:UploadPath", "uploads/cvs")
                ?? "uploads/cvs";
        }

        public async Task<string> SaveFileAsync(IFormFile file, string studentId)
        {
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, _uploadPath, studentId);

                // Create directory if it doesn't exist
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                // Return relative path
                return Path.Combine(_uploadPath, studentId, uniqueFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file for student: {StudentId}", studentId);
                throw;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);

                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<byte[]> GetFileAsync(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, filePath);

                if (!File.Exists(fullPath))
                    throw new FileNotFoundException("File not found", filePath);

                return await File.ReadAllBytesAsync(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file: {FilePath}", filePath);
                throw;
            }
        }

        public string GetFilePath(string fileName, string studentId)
        {
            return Path.Combine(_uploadPath, studentId, fileName);
        }
    }
}
