using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportStudentsToExcelAsync();
        Task<byte[]> ExportClusterToExcelAsync(int clusterId);
        Task<byte[]> ExportSkillsReportAsync();
    }
}
