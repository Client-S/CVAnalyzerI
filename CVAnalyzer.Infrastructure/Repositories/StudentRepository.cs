using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Interfaces;
using CVAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Student?> GetByStudentIdAsync(string studentId)
        {
            return await _dbSet
                .Include(s => s.StudentSkills)
                    .ThenInclude(ss => ss.Skill)
                .Include(s => s.Experiences)
                .Include(s => s.CVDocuments)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        public async Task<Student?> GetByIdWithDetailsAsync(int id)

        {

            return await _dbSet

                .Include(s => s.StudentSkills)

                    .ThenInclude(ss => ss.Skill)

                .Include(s => s.Experiences)

                .Include(s => s.CVDocuments)

                .FirstOrDefaultAsync(s => s.Id == id);

        }

        public async Task<IEnumerable<Student>> GetStudentsWithSkillsAsync()
        {
            return await _dbSet
                .Include(s => s.StudentSkills)
                    .ThenInclude(ss => ss.Skill)
                .Include(s => s.Experiences)
                .ToListAsync();
        }

        public async Task<IEnumerable<Student>> SearchBySkillAsync(string skill)
        {
            var normalizedSkill = skill.ToUpper();

            return await _dbSet
                .Include(s => s.StudentSkills)
                    .ThenInclude(ss => ss.Skill)
                .Where(s => s.StudentSkills.Any(ss =>
                    ss.Skill.NormalizedName.Contains(normalizedSkill) ||
                    ss.Skill.SkillName.Contains(skill)))
                .ToListAsync();
        }

        public async Task<bool> StudentIdExistsAsync(string studentId)
        {
            return await _dbSet.AnyAsync(s => s.StudentId == studentId);
        }
    }

}
