using Microsoft.Extensions.Caching.Memory;
using CVAnalyzer.Core.Interfaces;
using CVAnalyzer.Core.Entities;

namespace CVAnalyzer.Infrastructure.Services
{
    public class SkillCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly IUnitOfWork _unitOfWork;
        private const string SKILLS_CACHE_KEY = "all_skills";

        public SkillCacheService(IMemoryCache cache, IUnitOfWork unitOfWork)
        {
            _cache = cache;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Skill>> GetSkillsAsync()
        {
            if (_cache.TryGetValue(SKILLS_CACHE_KEY, out List<Skill> cachedSkills))
                return cachedSkills;

            var skills = (await _unitOfWork.Skills.GetAllAsync()).ToList();
            _cache.Set(SKILLS_CACHE_KEY, skills, TimeSpan.FromHours(1));
            return skills;
        }

        public void InvalidateCache()
        {
            _cache.Remove(SKILLS_CACHE_KEY);
        }
    }
}