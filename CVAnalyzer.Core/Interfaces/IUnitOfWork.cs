using CVAnalyzer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVAnalyzer.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        IRepository<Skill> Skills { get; }
        IRepository<Experience> Experiences { get; }
        IRepository<CVDocument> CVDocuments { get; }
        IRepository<StudentCluster> Clusters { get; }
        IRepository<AuditLog> AuditLogs { get; }
        IRepository<StudentSkill> StudentSkills { get; }
        IRepository<ClusterMember> ClusterMembers { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
