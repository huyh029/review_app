using reviewApi.Models;
using reviewApi.Service.Repositories.Auth;

namespace reviewApi.Service.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        // Users
        private IGenericRepository<User> _users;
        private IGenericRepository<Role> _roles;
        private IGenericRepository<Department> _departments;

        // Configuration - EvaluationObject
        private IGenericRepository<EvaluationObject> _evaluationObjects;
        private IGenericRepository<EvaluationObjectRole> _evaluationObjectRoles;

        // Configuration - EvaluationCriteria
        private IGenericRepository<CriteriaSet> _criteriaSets;
        private IGenericRepository<Criteria> _criterias;
        private IGenericRepository<CriteriaSetObject> _criteriaSetObjects;
        private IGenericRepository<Classification> _classifications;

        // Configuration - EvaluationFlow
        private IGenericRepository<EvaluationFlow> _evaluationFlows;
        private IGenericRepository<EvaluationFlowDepartment> _evaluationFlowDepartments;
        private IGenericRepository<EvaluationFlowRole> _evaluationFlowRoles;
        private IGenericRepository<EvaluationFlowObject> _evaluationFlowObjects;
        private IGenericRepository<EvaluationFlowCriteria> _evaluationFlowCriterias;

        // Configuration - ReportType
        private IGenericRepository<ReportType> _reportTypes;
        private IGenericRepository<ReportTypeCriteria> _reportTypeCriterias;

        // EvaluationBoard
        private IGenericRepository<Evaluation> _evaluations;
        private IGenericRepository<EvaluationScore> _evaluationScores;

        // EvaluationBoard - Comment
        private IGenericRepository<Comment> _comments;
        private IGenericRepository<CommentFile> _commentFiles;
        private IGenericRepository<CommentAudio> _commentAudios;
        private IGenericRepository<CommentReaction> _commentReactions;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        // Users — chỉ đọc từ DB
        public IGenericRepository<User>       Users       => _users       ??= new UserRepository(_context);
        public IGenericRepository<Role>       Roles       => _roles       ??= new RoleRepository(_context);
        public IGenericRepository<Department> Departments => _departments ??= new DepartmentRepository(_context);

        // Configuration - EvaluationObject
        public IGenericRepository<EvaluationObject> EvaluationObjects => _evaluationObjects ??= new GenericRepository<EvaluationObject>(_context);
        public IGenericRepository<EvaluationObjectRole> EvaluationObjectRoles => _evaluationObjectRoles ??= new GenericRepository<EvaluationObjectRole>(_context);

        // Configuration - EvaluationCriteria
        public IGenericRepository<CriteriaSet> CriteriaSets => _criteriaSets ??= new GenericRepository<CriteriaSet>(_context);
        public IGenericRepository<Criteria> Criterias => _criterias ??= new GenericRepository<Criteria>(_context);
        public IGenericRepository<CriteriaSetObject> CriteriaSetObjects => _criteriaSetObjects ??= new GenericRepository<CriteriaSetObject>(_context);
        public IGenericRepository<Classification> Classifications => _classifications ??= new GenericRepository<Classification>(_context);

        // Configuration - EvaluationFlow
        public IGenericRepository<EvaluationFlow> EvaluationFlows => _evaluationFlows ??= new GenericRepository<EvaluationFlow>(_context);
        public IGenericRepository<EvaluationFlowDepartment> EvaluationFlowDepartments => _evaluationFlowDepartments ??= new GenericRepository<EvaluationFlowDepartment>(_context);
        public IGenericRepository<EvaluationFlowRole> EvaluationFlowRoles => _evaluationFlowRoles ??= new GenericRepository<EvaluationFlowRole>(_context);
        public IGenericRepository<EvaluationFlowObject> EvaluationFlowObjects => _evaluationFlowObjects ??= new GenericRepository<EvaluationFlowObject>(_context);
        public IGenericRepository<EvaluationFlowCriteria> EvaluationFlowCriterias => _evaluationFlowCriterias ??= new GenericRepository<EvaluationFlowCriteria>(_context);

        // Configuration - ReportType
        public IGenericRepository<ReportType> ReportTypes => _reportTypes ??= new GenericRepository<ReportType>(_context);
        public IGenericRepository<ReportTypeCriteria> ReportTypeCriterias => _reportTypeCriterias ??= new GenericRepository<ReportTypeCriteria>(_context);

        // EvaluationBoard
        public IGenericRepository<Evaluation> Evaluations => _evaluations ??= new GenericRepository<Evaluation>(_context);
        public IGenericRepository<EvaluationScore> EvaluationScores => _evaluationScores ??= new GenericRepository<EvaluationScore>(_context);

        // EvaluationBoard - Comment
        public IGenericRepository<Comment> Comments => _comments ??= new GenericRepository<Comment>(_context);
        public IGenericRepository<CommentFile> CommentFiles => _commentFiles ??= new GenericRepository<CommentFile>(_context);
        public IGenericRepository<CommentAudio> CommentAudios => _commentAudios ??= new GenericRepository<CommentAudio>(_context);
        public IGenericRepository<CommentReaction> CommentReactions => _commentReactions ??= new GenericRepository<CommentReaction>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDisposable> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
