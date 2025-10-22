using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.DTOs.CV
{
    public class BulkCVUploadDto
    {
        [Required(ErrorMessage = "Please select CV files")]
        public List<IFormFile> CVFiles { get; set; } = new();

        public bool AutoExtractStudentInfo { get; set; } = true;
    }
}
