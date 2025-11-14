using CVAnalyzer.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CVAnalyzer.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<StudentSkill> StudentSkills { get; set; } = null!;
        public DbSet<Experience> Experiences { get; set; } = null!;
        public DbSet<CVDocument> CVDocuments { get; set; } = null!;
        public DbSet<StudentCluster> StudentClusters { get; set; } = null!;
        public DbSet<ClusterMember> ClusterMembers { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student Configuration
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StudentId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.HasIndex(e => e.StudentId).IsUnique();
                entity.HasIndex(e => e.Email);

                entity.HasOne(e => e.UploadedBy)
                    .WithMany(u => u.UploadedStudents)
                    .HasForeignKey(e => e.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Skill Configuration
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SkillName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.NormalizedName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.HasIndex(e => e.NormalizedName).IsUnique();
            });

            // StudentSkill Configuration (Many-to-Many)
            modelBuilder.Entity<StudentSkill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProficiencyLevel).HasMaxLength(50);
                entity.Property(e => e.ExtractedText).HasMaxLength(500);

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.StudentSkills)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Skill)
                    .WithMany(s => s.StudentSkills)
                    .HasForeignKey(e => e.SkillId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.StudentId, e.SkillId });

                modelBuilder.Entity<StudentSkill>()
                    .HasIndex(ss => ss.StudentId);

                modelBuilder.Entity<StudentSkill>()
                    .HasIndex(ss => ss.SkillId);
            });

            // Experience Configuration
            modelBuilder.Entity<Experience>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Company).HasMaxLength(200);
                entity.Property(e => e.Position).HasMaxLength(200);
                entity.Property(e => e.Duration).HasMaxLength(100);

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Experiences)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CVDocument Configuration
            modelBuilder.Entity<CVDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FilePath).HasMaxLength(1000);
                entity.Property(e => e.BlobPath).HasMaxLength(1000);
                entity.Property(e => e.FileType).HasMaxLength(20);
                entity.Property(e => e.ProcessingStatus).HasMaxLength(50);

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.CVDocuments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.ProcessingStatus);

                modelBuilder.Entity<CVDocument>()
                    .HasIndex(cd => cd.StudentId);
            });

            // StudentCluster Configuration
            modelBuilder.Entity<StudentCluster>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClusterName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Algorithm).HasMaxLength(50);

                entity.HasOne(e => e.CreatedBy)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ClusterMember Configuration
            modelBuilder.Entity<ClusterMember>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Cluster)
                    .WithMany(c => c.Members)
                    .HasForeignKey(e => e.ClusterId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.ClusterMemberships)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ClusterId, e.StudentId }).IsUnique();

                modelBuilder.Entity<ClusterMember>()
                    .HasIndex(cm => cm.ClusterId);
            });

            // AuditLog Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EntityType).HasMaxLength(100);
                entity.Property(e => e.EntityId).HasMaxLength(100);
                entity.Property(e => e.IpAddress).HasMaxLength(50);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AuditLogs)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.UserId, e.Timestamp });
            });

            // ApplicationUser additional configuration
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.Department).HasMaxLength(100);
            });
        }
    }
}
