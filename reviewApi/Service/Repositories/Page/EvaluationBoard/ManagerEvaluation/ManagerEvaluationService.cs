using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Models;
using reviewApi.Service.Page.EvaluationBoard.ManagerEvaluation;

namespace reviewApi.Service.Repositories.Page.EvaluationBoard.ManagerEvaluation
{
    public class ManagerEvaluationService : IManagerEvaluationService
    {
        private readonly IUnitOfWork _uow;

        public ManagerEvaluationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<EvaluationPaginatedResponse> GetAllAsync(Guid managerId, EvaluationFilterRequest filter)
        {
            var query = _uow.Evaluations.GetAll()
                .Where(e => e.ManagerId == managerId && (e.Status == "pending" || e.Status == "pending_director"));

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(e => e.Status == filter.Status);
            if (filter.Month.HasValue)
                query = query.Where(e => e.Month == filter.Month.Value);
            if (filter.Year.HasValue)
                query = query.Where(e => e.Year == filter.Year.Value);

            var total = query.Count();
            var items = query
                .OrderByDescending(e => e.Year).ThenByDescending(e => e.Month)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var users = _uow.Users.GetAll().ToDictionary(u => u.Id);
            var departments = _uow.Departments.GetAll().ToDictionary(d => d.Id);

            return new EvaluationPaginatedResponse
            {
                Data = items.Select(e =>
                {
                    users.TryGetValue(e.UserId, out var user);
                    var deptName = user != null && departments.TryGetValue(user.DepartmentId ?? Guid.Empty, out var dept) ? dept.DepartmentName : "";
                    return new EvaluationListItemDto
                    {
                        Id = e.Id,
                        FullName = user?.FullName ?? "",
                        Department = deptName,
                        Month = e.Month,
                        Year = e.Year,
                        EvaluationPeriod = $"Tháng {e.Month}/{e.Year}",
                        SelfScore = e.SelfScore,
                        ManagerScore = e.ManagerScore,
                        Status = e.Status
                    };
                }).ToList(),
                Pagination = new PaginationMeta
                {
                    CurrentPage = filter.Page,
                    TotalPages = (int)Math.Ceiling((double)total / filter.PageSize),
                    TotalItems = total,
                    ItemsPerPage = filter.PageSize
                }
            };
        }

        public async Task<EvaluationDetailDto> GetDetailAsync(Guid id)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            return BuildDetail(evaluation);
        }

        public async Task ReviewAsync(Guid id, Guid managerId)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            if (evaluation.Status != "pending")
                throw new Exception("Phiếu không ở trạng thái chờ duyệt");

            evaluation.Status = "draft";
            _uow.Evaluations.Update(evaluation);
            await _uow.SaveChangesAsync();
        }

        public async Task<EvaluationDetailDto> ApproveAsync(Guid id, Guid managerId, UpdateManagerScoresRequest request)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            if (evaluation.Status != "pending")
                throw new Exception("Phiếu không ở trạng thái chờ duyệt");

            var scores = _uow.EvaluationScores.Find(s => s.EvaluationId == id).ToList();
            foreach (var scoreInput in request.Scores)
            {
                var existing = scores.FirstOrDefault(s => s.VirtualCode == scoreInput.VirtualCode);
                if (existing != null)
                {
                    existing.ManagerScore = scoreInput.ManagerScore;
                    _uow.EvaluationScores.Update(existing);
                }
            }

            evaluation.ManagerScore = request.Scores.Sum(s => s.ManagerScore);
            evaluation.Status = "pending_director";
            _uow.Evaluations.Update(evaluation);
            await _uow.SaveChangesAsync();

            return BuildDetail(evaluation);
        }

        public async Task<EvaluationDetailDto> UpdateScoresAsync(Guid id, Guid managerId, UpdateManagerScoresRequest request)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            var scores = _uow.EvaluationScores.Find(s => s.EvaluationId == id).ToList();
            foreach (var scoreInput in request.Scores)
            {
                var existing = scores.FirstOrDefault(s => s.VirtualCode == scoreInput.VirtualCode);
                if (existing != null)
                {
                    existing.ManagerScore = scoreInput.ManagerScore;
                    _uow.EvaluationScores.Update(existing);
                }
            }

            evaluation.ManagerScore = request.Scores.Sum(s => s.ManagerScore);
            _uow.Evaluations.Update(evaluation);
            await _uow.SaveChangesAsync();

            return BuildDetail(evaluation);
        }

        public async Task CompleteAsync(Guid id, Guid managerId)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            if (evaluation.Status != "pending_director")
                throw new Exception("Phiếu không ở trạng thái chờ giám đốc duyệt");

            evaluation.Status = "completed";
            _uow.Evaluations.Update(evaluation);
            await _uow.SaveChangesAsync();
        }

        public List<object> GetManagers(Guid currentUserId, Guid criteriaSetId)
        {
            var currentUser = _uow.Users.FindFirst(u => u.Id == currentUserId)
                ?? throw new Exception("Không tìm thấy người dùng");

            var currentUserObjectIds = _uow.EvaluationObjectRoles.GetAll()
                .Where(eor => eor.UserId == currentUserId)
                .Select(eor => eor.EvaluationObjectId)
                .ToList();

            var flow = _uow.EvaluationFlows.FindWithInclude(
                    f => f.IsActive == 1
                        && f.Departments.Any(d => d.DepartmentId == currentUser.DepartmentId)
                        && f.Criterias.Any(c => c.CriteriaSetId == criteriaSetId)
                        && f.Roles.Any(r => r.RoleId == currentUser.RoleId)
                        && f.Objects.Any(o => currentUserObjectIds.Contains(o.EvaluationObjectId)),
                    f => f.Departments,
                    f => f.Criterias,
                    f => f.Roles,
                    f => f.Objects)
                .FirstOrDefault();

            if (flow == null) return new List<object>();

            var currentUserFlowRole = flow.Roles.FirstOrDefault(r => r.RoleId == currentUser.RoleId);
            if (currentUserFlowRole == null) return new List<object>();

            var rolePrefix = currentUserFlowRole.VirtualCode + ".";
            var childRoleIds = flow.Roles
                .Where(r => r.VirtualCode.StartsWith(rolePrefix))
                .Select(r => r.RoleId)
                .ToHashSet();

            var currentUserFlowObjects = flow.Objects
                .Where(o => currentUserObjectIds.Contains(o.EvaluationObjectId))
                .ToList();

            var childObjectIds = new HashSet<Guid>();
            foreach (var flowObj in currentUserFlowObjects)
            {
                var objPrefix = flowObj.VirtualCode + ".";
                foreach (var id in flow.Objects
                    .Where(o => o.VirtualCode.StartsWith(objPrefix))
                    .Select(o => o.EvaluationObjectId))
                    childObjectIds.Add(id);
            }

            var validUserIds = _uow.EvaluationObjectRoles.GetAll()
                .Where(eor => childObjectIds.Contains(eor.EvaluationObjectId))
                .Select(eor => eor.UserId)
                .ToHashSet();

            return _uow.Users.GetAll()
                .Where(u => u.Id != currentUserId
                    && u.DepartmentId == currentUser.DepartmentId
                    && u.RoleId.HasValue && childRoleIds.Contains(u.RoleId.Value)
                    && validUserIds.Contains(u.Id))
                .OrderBy(u => u.FullName)
                .Select(u => (object)new { id = u.Id, fullName = u.FullName })
                .ToList();
        }

        private EvaluationDetailDto BuildDetail(Evaluation evaluation)
        {
            var user = _uow.Users.GetById(evaluation.UserId);
            var deptName = "";
            if (user != null)
            {
                var dept = _uow.Departments.FindFirst(d => d.Id == user.DepartmentId);
                deptName = dept?.DepartmentName ?? "";
            }

            var scores = _uow.EvaluationScores.Find(s => s.EvaluationId == evaluation.Id).ToList();
            var criterias = _uow.Criterias.Find(c => c.CriteriaSetId == evaluation.CriteriaSetId && c.IsActive == 1).ToList();
            var classifications = _uow.Classifications.Find(c => c.CriteriaSetId == evaluation.CriteriaSetId && c.IsActive == 1).ToList();

            var isPending = evaluation.Status == "pending";

            return new EvaluationDetailDto
            {
                Id = evaluation.Id,
                UserId = evaluation.UserId,
                FullName = user?.FullName ?? "",
                Department = deptName,
                Month = evaluation.Month,
                Year = evaluation.Year,
                CriteriaSetId = evaluation.CriteriaSetId,
                SelfScore = evaluation.SelfScore,
                ManagerScore = isPending ? evaluation.SelfScore : evaluation.ManagerScore,
                Status = evaluation.Status,
                Scores = scores.Select(s => new EvaluationScoreDto
                {
                    Id = s.Id,
                    VirtualCode = s.VirtualCode,
                    SelfScore = s.SelfScore,
                    ManagerScore = isPending ? s.SelfScore : s.ManagerScore
                }).ToList(),
                CriteriaTree = EvaluationHelper.BuildCriteriaTree(criterias),
                Classifications = classifications.OrderBy(cl => cl.MinScore).Select(cl => new ClassificationDto
                {
                    Code = cl.Code,
                    Name = cl.Name,
                    Abbreviation = cl.Abbreviation,
                    MinScore = cl.MinScore,
                    MaxScore = cl.MaxScore
                }).ToList()
            };
        }
    }
}

