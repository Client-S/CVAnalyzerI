using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Interfaces;
using CVAnalyzer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Infrastructure.Repositories
{
    public class StudentRepository : Repository<Student>, IStudentRepository
    {
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private const string STUDENTS_WITH_SKILLS_CACHE_KEY = "AllStudentsWithSkills";
        private const int CACHE_TTL_MINUTES = 5;

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
            // Check cache first
            if (_cache.TryGetValue(STUDENTS_WITH_SKILLS_CACHE_KEY, out IEnumerable<Student>? cachedStudents))
            {
                return cachedStudents!;
            }

            // Load from database with full eager loading
            var students = await _dbSet
                .Include(s => s.StudentSkills)
                    .ThenInclude(ss => ss.Skill)
                .Include(s => s.Experiences)
                .Include(s => s.CVDocuments) // Also include CVDocuments
                .ToListAsync();

            // Cache the results
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(CACHE_TTL_MINUTES));

            _cache.Set(STUDENTS_WITH_SKILLS_CACHE_KEY, students, cacheOptions);

            return students;
        }

        // Method to invalidate cache when students are modified
        public void InvalidateStudentCache()
        {
            _cache.Remove(STUDENTS_WITH_SKILLS_CACHE_KEY);
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
