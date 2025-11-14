using CVAnalyzer.Core.Entities;
using CVAnalyzer.Core.Interfaces;
using CVAnalyzer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace CVAnalyzer.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        private IStudentRepository? _students;
        private IRepository<Skill>? _skills;
        private IRepository<Experience>? _experiences;
        private IRepository<CVDocument>? _cvDocuments;
        private IRepository<StudentCluster>? _clusters;
        private IRepository<AuditLog>? _auditLogs; 
        private IRepository<StudentSkill>? _studentSkills;
        private IRepository<ClusterMember>? _clusterMembers;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IStudentRepository Students =>
            _students ??= new StudentRepository(_context);

        public IRepository<Skill> Skills =>
            _skills ??= new Repository<Skill>(_context);

        public IRepository<Experience> Experiences =>
            _experiences ??= new Repository<Experience>(_context);

        public IRepository<CVDocument> CVDocuments =>
            _cvDocuments ??= new Repository<CVDocument>(_context);

        public IRepository<StudentCluster> Clusters =>
            _clusters ??= new Repository<StudentCluster>(_context);

        public IRepository<AuditLog> AuditLogs =>
            _auditLogs ??= new Repository<AuditLog>(_context);

        public IRepository<StudentSkill> StudentSkills =>
            _studentSkills ??= new Repository<StudentSkill>(_context);

        public IRepository<ClusterMember> ClusterMembers =>
            _clusterMembers ??= new Repository<ClusterMember>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
