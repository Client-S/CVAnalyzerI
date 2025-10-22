using CVAnalyzer.Application.DTOs.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Application.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync();
        Task<StudentDto?> GetStudentByIdAsync(int id);
        Task<StudentDto?> GetStudentByStudentIdAsync(string studentId);
        Task<StudentDto> CreateStudentAsync(CreateStudentDto createDto);
        Task<bool> UpdateStudentAsync(UpdateStudentDto updateDto);
        Task<bool> DeleteStudentAsync(int id);
        Task<IEnumerable<StudentDto>> SearchStudentsAsync(string searchTerm);
        Task<IEnumerable<StudentDto>> GetStudentsBySkillAsync(string skill);
    }
}
