using Microsoft.EntityFrameworkCore;

namespace reviewApi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Users
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }

        // Configuration - EvaluationObject
        public DbSet<EvaluationObject> EvaluationObjects { get; set; }
        public DbSet<EvaluationObjectRole> EvaluationObjectRoles { get; set; }

        // Configuration - EvaluationCriteria
        public DbSet<CriteriaSet> CriteriaSets { get; set; }
        public DbSet<Criteria> Criterias { get; set; }
        public DbSet<Classification> Classifications { get; set; }
        public DbSet<CriteriaSetObject> CriteriaSetObjects { get; set; }

        // Configuration - EvaluationFlow
        public DbSet<EvaluationFlow> EvaluationFlows { get; set; }
        public DbSet<EvaluationFlowDepartment> EvaluationFlowDepartments { get; set; }
        public DbSet<EvaluationFlowRole> EvaluationFlowRoles { get; set; }
        public DbSet<EvaluationFlowObject> EvaluationFlowObjects { get; set; }
        public DbSet<EvaluationFlowCriteria> EvaluationFlowCriterias { get; set; }

        // Configuration - ReportType
        public DbSet<ReportType> ReportTypes { get; set; }
        public DbSet<ReportTypeCriteria> ReportTypeCriterias { get; set; }

        // EvaluationBoard
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<EvaluationScore> EvaluationScores { get; set; }

        // EvaluationBoard - Comment
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentFile> CommentFiles { get; set; }
        public DbSet<CommentAudio> CommentAudios { get; set; }
        public DbSet<CommentReaction> CommentReactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // === Users Configuration ===
            modelBuilder.Entity<User>().HasKey(e => e.Id);
            modelBuilder.Entity<User>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            modelBuilder.Entity<Role>().HasKey(e => e.Id);
            modelBuilder.Entity<Role>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<Role>().HasIndex(e => e.RoleCode).IsUnique();

            modelBuilder.Entity<Department>().HasKey(e => e.Id);
            modelBuilder.Entity<Department>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<Department>().HasIndex(e => e.DepartmentCode).IsUnique();

            // User - Role
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // User - Department
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Department - Department (Parent-Child)
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Parent)
                .WithMany(d => d.Children)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // === EvaluationObject Configuration ===
            modelBuilder.Entity<EvaluationObject>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationObject>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<EvaluationObject>().HasIndex(e => e.Code).IsUnique();

            modelBuilder.Entity<EvaluationObjectRole>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationObjectRole>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            // EvaluationObjectRole - EvaluationObject
            modelBuilder.Entity<EvaluationObjectRole>()
                .HasOne(eor => eor.EvaluationObject)
                .WithMany(eo => eo.EvaluationObjectRoles)
                .HasForeignKey(eor => eor.EvaluationObjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // EvaluationObjectRole - User
            modelBuilder.Entity<EvaluationObjectRole>()
                .HasOne(eor => eor.User)
                .WithMany()
                .HasForeignKey(eor => eor.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // === CriteriaSet Configuration ===
            modelBuilder.Entity<CriteriaSet>().HasKey(e => e.Id);
            modelBuilder.Entity<CriteriaSet>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            // === Criteria Configuration ===
            modelBuilder.Entity<Criteria>().HasKey(e => e.Id);
            modelBuilder.Entity<Criteria>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<Criteria>().HasIndex(c => new { c.CriteriaSetId, c.VirtualCode }).IsUnique();

            // Criteria - CriteriaSet
            modelBuilder.Entity<Criteria>()
                .HasOne(c => c.CriteriaSet)
                .WithMany(cs => cs.Criterias)
                .HasForeignKey(c => c.CriteriaSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Classification Configuration ===
            modelBuilder.Entity<Classification>().HasKey(e => e.Id);
            modelBuilder.Entity<Classification>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<Classification>().HasIndex(c => new { c.CriteriaSetId, c.Code }).IsUnique();

            // Classification - CriteriaSet
            modelBuilder.Entity<Classification>()
                .HasOne(c => c.CriteriaSet)
                .WithMany(cs => cs.Classifications)
                .HasForeignKey(c => c.CriteriaSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // === CriteriaSetObject Configuration ===
            modelBuilder.Entity<CriteriaSetObject>().HasKey(e => e.Id);
            modelBuilder.Entity<CriteriaSetObject>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<CriteriaSetObject>().HasIndex(cso => new { cso.CriteriaSetId, cso.EvaluationObjectId }).IsUnique();

            // CriteriaSetObject - CriteriaSet
            modelBuilder.Entity<CriteriaSetObject>()
                .HasOne(cso => cso.CriteriaSet)
                .WithMany(cs => cs.CriteriaSetObjects)
                .HasForeignKey(cso => cso.CriteriaSetId)
                .OnDelete(DeleteBehavior.Cascade);

            // CriteriaSetObject - EvaluationObject
            modelBuilder.Entity<CriteriaSetObject>()
                .HasOne(cso => cso.EvaluationObject)
                .WithMany()
                .HasForeignKey(cso => cso.EvaluationObjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // === EvaluationFlow Configuration ===
            modelBuilder.Entity<EvaluationFlow>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationFlow>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<EvaluationFlow>().HasIndex(e => e.FlowCode).IsUnique();

            // === EvaluationFlowDepartment Configuration ===
            modelBuilder.Entity<EvaluationFlowDepartment>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationFlowDepartment>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<EvaluationFlowDepartment>().HasIndex(efd => new { efd.FlowId, efd.DepartmentId }).IsUnique();

            // EvaluationFlowDepartment - EvaluationFlow
            modelBuilder.Entity<EvaluationFlowDepartment>()
                .HasOne(efd => efd.EvaluationFlow)
                .WithMany(ef => ef.Departments)
                .HasForeignKey(efd => efd.FlowId)
                .OnDelete(DeleteBehavior.Cascade);

            // EvaluationFlowDepartment - Department
            modelBuilder.Entity<EvaluationFlowDepartment>()
                .HasOne(efd => efd.Department)
                .WithMany()
                .HasForeignKey(efd => efd.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // === EvaluationFlowRole Configuration ===
            modelBuilder.Entity<EvaluationFlowRole>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationFlowRole>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<EvaluationFlowRole>().HasIndex(efr => new { efr.FlowId, efr.VirtualCode }).IsUnique();

            // EvaluationFlowRole - EvaluationFlow
            modelBuilder.Entity<EvaluationFlowRole>()
                .HasOne(efr => efr.EvaluationFlow)
                .WithMany(ef => ef.Roles)
                .HasForeignKey(efr => efr.FlowId)
                .OnDelete(DeleteBehavior.Cascade);

            // EvaluationFlowRole - Role
            modelBuilder.Entity<EvaluationFlowRole>()
                .HasOne(efr => efr.Role)
                .WithMany()
                .HasForeignKey(efr => efr.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // === EvaluationFlowObject Configuration ===
            modelBuilder.Entity<EvaluationFlowObject>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationFlowObject>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<EvaluationFlowObject>().HasIndex(efo => new { efo.FlowId, efo.VirtualCode }).IsUnique();

            // EvaluationFlowObject - EvaluationFlow
            modelBuilder.Entity<EvaluationFlowObject>()
                .HasOne(efo => efo.EvaluationFlow)
                .WithMany(ef => ef.Objects)
                .HasForeignKey(efo => efo.FlowId)
                .OnDelete(DeleteBehavior.Cascade);

            // EvaluationFlowObject - EvaluationObject
            modelBuilder.Entity<EvaluationFlowObject>()
                .HasOne(efo => efo.EvaluationObject)
                .WithMany()
                .HasForeignKey(efo => efo.EvaluationObjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // === EvaluationFlowCriteria Configuration ===
            modelBuilder.Entity<EvaluationFlowCriteria>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationFlowCriteria>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<EvaluationFlowCriteria>().HasIndex(efc => new { efc.FlowId, efc.CriteriaSetId }).IsUnique();

            // EvaluationFlowCriteria - EvaluationFlow
            modelBuilder.Entity<EvaluationFlowCriteria>()
                .HasOne(efc => efc.EvaluationFlow)
                .WithMany(ef => ef.Criterias)
                .HasForeignKey(efc => efc.FlowId)
                .OnDelete(DeleteBehavior.Cascade);

            // EvaluationFlowCriteria - CriteriaSet
            modelBuilder.Entity<EvaluationFlowCriteria>()
                .HasOne(efc => efc.CriteriaSet)
                .WithMany()
                .HasForeignKey(efc => efc.CriteriaSetId)
                .OnDelete(DeleteBehavior.Restrict);

            // === ReportType Configuration ===
            modelBuilder.Entity<ReportType>().HasKey(e => e.Id);
            modelBuilder.Entity<ReportType>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<ReportType>().HasIndex(e => e.Code).IsUnique();

            // === ReportTypeCriteria Configuration ===
            modelBuilder.Entity<ReportTypeCriteria>().HasKey(e => e.Id);
            modelBuilder.Entity<ReportTypeCriteria>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");
            modelBuilder.Entity<ReportTypeCriteria>().HasIndex(rtc => new { rtc.ReportTypeId, rtc.CriteriaSetId }).IsUnique();

            // ReportTypeCriteria - ReportType
            modelBuilder.Entity<ReportTypeCriteria>()
                .HasOne(rtc => rtc.ReportType)
                .WithMany(rt => rt.ReportTypeCriterias)
                .HasForeignKey(rtc => rtc.ReportTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ReportTypeCriteria - CriteriaSet
            modelBuilder.Entity<ReportTypeCriteria>()
                .HasOne(rtc => rtc.CriteriaSet)
                .WithMany()
                .HasForeignKey(rtc => rtc.CriteriaSetId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Evaluation Configuration ===
            modelBuilder.Entity<Evaluation>().HasKey(e => e.Id);
            modelBuilder.Entity<Evaluation>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            // Evaluation - User
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Evaluation - Manager (User)
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Evaluation - CriteriaSet
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.CriteriaSet)
                .WithMany()
                .HasForeignKey(e => e.CriteriaSetId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for Evaluation
            modelBuilder.Entity<Evaluation>()
                .HasIndex(e => new { e.UserId, e.Year, e.Month });

            // === EvaluationScore Configuration ===
            modelBuilder.Entity<EvaluationScore>().HasKey(e => e.Id);
            modelBuilder.Entity<EvaluationScore>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            // EvaluationScore - Evaluation
            modelBuilder.Entity<EvaluationScore>()
                .HasOne(es => es.Evaluation)
                .WithMany(e => e.EvaluationScores)
                .HasForeignKey(es => es.EvaluationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for EvaluationScore
            modelBuilder.Entity<EvaluationScore>()
                .HasIndex(es => new { es.EvaluationId, es.VirtualCode })
                .IsUnique();

            // === Comment Configuration ===
            modelBuilder.Entity<Comment>().HasKey(e => e.Id);
            modelBuilder.Entity<Comment>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            // Comment - Evaluation
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Evaluation)
                .WithMany()
                .HasForeignKey(c => c.EvaluationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Comment - User
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Comment - Comment (Reply)
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ReplyToComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ReplyToCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for Comment
            modelBuilder.Entity<Comment>()
                .HasIndex(c => new { c.EvaluationId, c.CreatedAt });

            // === CommentFile Configuration ===
            modelBuilder.Entity<CommentFile>().HasKey(e => e.Id);
            modelBuilder.Entity<CommentFile>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            // CommentFile - Comment
            modelBuilder.Entity<CommentFile>()
                .HasOne(cf => cf.Comment)
                .WithMany(c => c.Files)
                .HasForeignKey(cf => cf.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // === CommentAudio Configuration ===
            modelBuilder.Entity<CommentAudio>().HasKey(e => e.Id);
            modelBuilder.Entity<CommentAudio>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            // CommentAudio - Comment
            modelBuilder.Entity<CommentAudio>()
                .HasOne(ca => ca.Comment)
                .WithMany(c => c.Audios)
                .HasForeignKey(ca => ca.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // === CommentReaction Configuration ===
            modelBuilder.Entity<CommentReaction>().HasKey(e => e.Id);
            modelBuilder.Entity<CommentReaction>().Property(e => e.Id).HasDefaultValueSql("SYS_GUID()");

            // CommentReaction - Comment
            modelBuilder.Entity<CommentReaction>()
                .HasOne(cr => cr.Comment)
                .WithMany(c => c.Reactions)
                .HasForeignKey(cr => cr.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            // CommentReaction - User
            modelBuilder.Entity<CommentReaction>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique index for CommentReaction
            modelBuilder.Entity<CommentReaction>()
                .HasIndex(cr => new { cr.CommentId, cr.UserId, cr.Emoji })
                .IsUnique();
        }
    }
}
