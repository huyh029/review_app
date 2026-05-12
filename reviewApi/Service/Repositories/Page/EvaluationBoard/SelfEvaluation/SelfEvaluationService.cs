using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Models;
using reviewApi.Service.Page.EvaluationBoard.SelfEvaluation;

namespace reviewApi.Service.Repositories.Page.EvaluationBoard.SelfEvaluation
{
    public class SelfEvaluationService : ISelfEvaluationService
    {
        private readonly IUnitOfWork _uow;

        public SelfEvaluationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<EvaluationPaginatedResponse> GetAllAsync(Guid userId, EvaluationFilterRequest filter)
        {
            var query = _uow.Evaluations.GetAll()
                .Where(e => e.UserId == userId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(e => e.Status == filter.Status);
            if (filter.Month.HasValue)
                query = query.Where(e => e.Month == filter.Month.Value);
            if (filter.Year.HasValue)
                query = query.Where(e => e.Year == filter.Year.Value);

            var total = query.Count();
            var selectableCount = query.Count(e => e.Status == "draft");
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
                    ItemsPerPage = filter.PageSize,
                    SelectableCount = selectableCount
                }
            };
        }

        public async Task<EvaluationDetailDto> GetDetailAsync(Guid id)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            return BuildDetail(evaluation);
        }

        public async Task<EvaluationDetailDto> CreateAsync(Guid userId, CreateEvaluationRequest request)
        {
            var criteriaSet = _uow.CriteriaSets.GetById(request.CriteriaSetId)
                ?? throw new Exception("Không tìm thấy bộ tiêu chí");

            var evaluation = new Evaluation
            {
                UserId = userId,
                Month = request.Month,
                Year = request.Year,
                CriteriaSetId = request.CriteriaSetId,
                Status = "draft",
                SelfScore = request.Scores.Sum(s => s.SelfScore)
            };

            _uow.Evaluations.Add(evaluation);
            await _uow.SaveChangesAsync();

            foreach (var score in request.Scores)
            {
                _uow.EvaluationScores.Add(new EvaluationScore
                {
                    EvaluationId = evaluation.Id,
                    VirtualCode = score.VirtualCode,
                    SelfScore = score.SelfScore
                });
            }

            await _uow.SaveChangesAsync();
            return BuildDetail(evaluation);
        }

        public async Task<EvaluationDetailDto> UpdateAsync(Guid id, Guid userId, UpdateEvaluationRequest request)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id && e.UserId == userId)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            if (evaluation.Status != "draft")
                throw new Exception("Chỉ có thể chỉnh sửa phiếu ở trạng thái dự thảo");

            var oldScores = _uow.EvaluationScores.Find(s => s.EvaluationId == id).ToList();
            _uow.EvaluationScores.RemoveRange(oldScores);

            foreach (var score in request.Scores)
            {
                _uow.EvaluationScores.Add(new EvaluationScore
                {
                    EvaluationId = id,
                    VirtualCode = score.VirtualCode,
                    SelfScore = score.SelfScore
                });
            }

            evaluation.SelfScore = request.Scores.Sum(s => s.SelfScore);
            _uow.Evaluations.Update(evaluation);
            await _uow.SaveChangesAsync();

            return BuildDetail(evaluation);
        }

        public async Task<EvaluationDetailDto> SaveAndSubmitAsync(Guid userId, CreateEvaluationRequest request, Guid? managerId = null)
        {
            var criteriaSet = _uow.CriteriaSets.GetById(request.CriteriaSetId)
                ?? throw new Exception("Không tìm thấy bộ tiêu chí");

            var evaluation = new Evaluation
            {
                UserId = userId,
                Month = request.Month,
                Year = request.Year,
                CriteriaSetId = request.CriteriaSetId,
                Status = "pending",
                SelfScore = request.Scores.Sum(s => s.SelfScore),
                ManagerId = managerId
            };

            _uow.Evaluations.Add(evaluation);
            await _uow.SaveChangesAsync();

            foreach (var score in request.Scores)
            {
                _uow.EvaluationScores.Add(new EvaluationScore
                {
                    EvaluationId = evaluation.Id,
                    VirtualCode = score.VirtualCode,
                    SelfScore = score.SelfScore
                });
            }

            await _uow.SaveChangesAsync();
            return BuildDetail(evaluation);
        }

        public async Task SubmitAsync(Guid id, Guid userId, Guid? managerId = null)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id && e.UserId == userId)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            if (evaluation.Status != "draft")
                throw new Exception("Chỉ có thể nộp phiếu ở trạng thái dự thảo");

            evaluation.Status = "pending";
            if (managerId.HasValue)
                evaluation.ManagerId = managerId.Value;

            _uow.Evaluations.Update(evaluation);
            await _uow.SaveChangesAsync();
        }

        public async Task RecallAsync(Guid id, Guid userId)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id && e.UserId == userId)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            if (evaluation.Status != "pending")
                throw new Exception("Chỉ có thể thu hồi phiếu ở trạng thái chờ đánh giá");

            evaluation.Status = "draft";
            _uow.Evaluations.Update(evaluation);
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteManyAsync(DeleteManyRequest request, Guid userId)
        {
            var query = _uow.Evaluations.GetAll()
                .Where(e => e.UserId == userId && e.Status == "draft");

            if (request.IsAll)
            {
                if (request.Filter != null)
                {
                    if (!string.IsNullOrEmpty(request.Filter.Status))
                        query = query.Where(e => e.Status == request.Filter.Status);
                    if (request.Filter.Month.HasValue)
                        query = query.Where(e => e.Month == request.Filter.Month.Value);
                    if (request.Filter.Year.HasValue)
                        query = query.Where(e => e.Year == request.Filter.Year.Value);
                }
                if (request.ExcludeIds.Any())
                    query = query.Where(e => !request.ExcludeIds.Contains(e.Id));
            }
            else
            {
                query = query.Where(e => request.Ids.Contains(e.Id));
            }

            var evaluations = query.ToList();
            foreach (var evaluation in evaluations)
            {
                var scores = _uow.EvaluationScores.Find(s => s.EvaluationId == evaluation.Id).ToList();
                _uow.EvaluationScores.RemoveRange(scores);
                _uow.Evaluations.Remove(evaluation);
            }
            await _uow.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var evaluation = _uow.Evaluations.FindFirst(e => e.Id == id && e.UserId == userId)
                ?? throw new Exception("Không tìm thấy phiếu đánh giá");

            if (evaluation.Status != "draft")
                throw new Exception("Chỉ có thể xóa phiếu ở trạng thái dự thảo");

            var scores = _uow.EvaluationScores.Find(s => s.EvaluationId == id).ToList();
            _uow.EvaluationScores.RemoveRange(scores);
            _uow.Evaluations.Remove(evaluation);
            await _uow.SaveChangesAsync();
        }

        public async Task<NewEvaluationTemplateDto> GetNewTemplateAsync(Guid userId, int? month = null, int? year = null, Guid? criteriaId = null)
        {
            var user = _uow.Users.GetById(userId)
                ?? throw new Exception("Không tìm thấy người dùng");

            var dept = _uow.Departments.FindFirst(d => d.Id == user.DepartmentId);

            int targetMonth = month ?? DateTime.Now.Month;
            int targetYear = year ?? DateTime.Now.Year;

            var objectIds = _uow.EvaluationObjectRoles
                .Find(r => r.UserId == userId)
                .Select(r => r.EvaluationObjectId)
                .Distinct()
                .ToList();

            if (!objectIds.Any())
                throw new Exception("Người dùng chưa được gán đối tượng đánh giá");

            var criteriaSetIds = _uow.CriteriaSetObjects
                .Find(cso => objectIds.Contains(cso.EvaluationObjectId))
                .Select(cso => cso.CriteriaSetId)
                .Distinct()
                .ToList();

            if (!criteriaSetIds.Any())
                throw new Exception("Không tìm thấy bộ tiêu chí cho đối tượng đánh giá này");

            var allCriteriaSets = _uow.CriteriaSets
                .Find(cs => criteriaSetIds.Contains(cs.Id) && cs.IsActive == 1)
                .ToList();

            if (!allCriteriaSets.Any())
                throw new Exception("Không tìm thấy bộ tiêu chí đang hoạt động");

            var criteriaSet = allCriteriaSets.FirstOrDefault(cs =>
            {
                bool monthMatch = true, yearMatch = true;
                if (!string.IsNullOrEmpty(cs.ApplicableMonths))
                {
                    var months = cs.ApplicableMonths.Split(',').Select(m => int.TryParse(m.Trim(), out var v) ? v : -1).ToList();
                    monthMatch = months.Contains(targetMonth);
                }
                if (!string.IsNullOrEmpty(cs.ApplicableYears))
                {
                    var years = cs.ApplicableYears.Split(',').Select(y => int.TryParse(y.Trim(), out var v) ? v : -1).ToList();
                    yearMatch = years.Contains(targetYear);
                }
                return monthMatch && yearMatch;
            });

            bool matched = criteriaSet != null;
            criteriaSet ??= allCriteriaSets.First();

            if (!matched)
                return new NewEvaluationTemplateDto { IsChanged = false, IsHaveCriteria = false };

            if (criteriaId.HasValue && criteriaId.Value == criteriaSet.Id)
                return new NewEvaluationTemplateDto { IsChanged = false, IsHaveCriteria = true };

            var criterias = _uow.Criterias.Find(c => c.CriteriaSetId == criteriaSet.Id && c.IsActive == 1).ToList();
            var classifications = _uow.Classifications.Find(c => c.CriteriaSetId == criteriaSet.Id && c.IsActive == 1).ToList();

            return new NewEvaluationTemplateDto
            {
                IsChanged = true,
                IsHaveCriteria = true,
                CriteriaSetId = criteriaSet.Id,
                CriteriaSetName = criteriaSet.Name,
                FullName = user.FullName,
                Department = dept?.DepartmentName ?? "",
                CurrentMonth = targetMonth,
                CurrentYear = targetYear,
                CriteriaTree = EvaluationHelper.BuildCriteriaTree(criterias, defaultSelfScore: true),
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

        private EvaluationDetailDto BuildDetail(Evaluation evaluation)
        {
            var user = _uow.Users.FindFirst(u => u.Id == evaluation.UserId);
            var deptName = "";
            if (user != null)
            {
                var dept = _uow.Departments.FindFirst(d => d.Id == user.DepartmentId);
                deptName = dept?.DepartmentName ?? "";
            }

            var scores = _uow.EvaluationScores.Find(s => s.EvaluationId == evaluation.Id).ToList();
            var scoreMap = scores.ToDictionary(s => s.VirtualCode);
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
                CriteriaTree = EvaluationHelper.BuildCriteriaTree(criterias, scoreMap),
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

