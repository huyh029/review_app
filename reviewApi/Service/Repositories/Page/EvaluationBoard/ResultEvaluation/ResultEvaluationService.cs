using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Models;
using reviewApi.Service.Page.EvaluationBoard.ResultEvaluation;

namespace reviewApi.Service.Repositories.Page.EvaluationBoard.ResultEvaluation
{
    public class ResultEvaluationService : IResultEvaluationService
    {
        private readonly IUnitOfWork _uow;

        public ResultEvaluationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<EvaluationPaginatedResponse> GetAllAsync(Guid userId, EvaluationFilterRequest filter)
        {
            var query = _uow.Evaluations.GetAll()
                .Where(e => e.Status == "completed");

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
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id && e.Status == "completed")
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

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
                ManagerScore = evaluation.ManagerScore,
                Status = evaluation.Status,
                Scores = scores.Select(s => new EvaluationScoreDto
                {
                    Id = s.Id,
                    VirtualCode = s.VirtualCode,
                    SelfScore = s.SelfScore,
                    ManagerScore = s.ManagerScore
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

