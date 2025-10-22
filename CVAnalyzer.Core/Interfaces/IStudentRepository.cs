using CVAnalyzer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Interfaces
{
    public interface IStudentRepository : IRepository<Student>
    {
        Task<Student?> GetByStudentIdAsync(string studentId);
        Task<IEnumerable<Student>> GetStudentsWithSkillsAsync();
        Task<IEnumerable<Student>> SearchBySkillAsync(string skill);
        Task<bool> StudentIdExistsAsync(string studentId);
    }
}
