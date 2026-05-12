using reviewApi.Models;

namespace reviewApi.Service
{
    public interface IUnitOfWork : IDisposable
    {
        // Users
        IGenericRepository<User> Users { get; }
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<Department> Departments { get; }

        // Configuration - EvaluationObject
        IGenericRepository<EvaluationObject> EvaluationObjects { get; }
        IGenericRepository<EvaluationObjectRole> EvaluationObjectRoles { get; }

        // Configuration - EvaluationCriteria
        IGenericRepository<CriteriaSet> CriteriaSets { get; }
        IGenericRepository<Criteria> Criterias { get; }
        IGenericRepository<CriteriaSetObject> CriteriaSetObjects { get; }
        IGenericRepository<Classification> Classifications { get; }

        // Configuration - EvaluationFlow
        IGenericRepository<EvaluationFlow> EvaluationFlows { get; }
        IGenericRepository<EvaluationFlowDepartment> EvaluationFlowDepartments { get; }
        IGenericRepository<EvaluationFlowRole> EvaluationFlowRoles { get; }
        IGenericRepository<EvaluationFlowObject> EvaluationFlowObjects { get; }
        IGenericRepository<EvaluationFlowCriteria> EvaluationFlowCriterias { get; }

        // Configuration - ReportType
        IGenericRepository<ReportType> ReportTypes { get; }
        IGenericRepository<ReportTypeCriteria> ReportTypeCriterias { get; }

        // EvaluationBoard
        IGenericRepository<Evaluation> Evaluations { get; }
        IGenericRepository<EvaluationScore> EvaluationScores { get; }

        // EvaluationBoard - Comment
        IGenericRepository<Comment> Comments { get; }
        IGenericRepository<CommentFile> CommentFiles { get; }
        IGenericRepository<CommentAudio> CommentAudios { get; }
        IGenericRepository<CommentReaction> CommentReactions { get; }

        Task<int> SaveChangesAsync();
        Task<IDisposable> BeginTransactionAsync();
    }
}
