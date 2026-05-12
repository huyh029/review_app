using reviewApi.DTO.Page.Configuration;
using reviewApi.Models;
using reviewApi.Service.Page.Configuration;
using reviewApi.Service.General;

namespace reviewApi.Service.Repositories.Page.Configuration
{
    public class EvaluationCriteriaDetailService : IEvaluationCriteriaDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvaluationCriteriaDetailService> _logger;

        public EvaluationCriteriaDetailService(IUnitOfWork unitOfWork, ILogger<EvaluationCriteriaDetailService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<EvaluationObjectDto>> GetActiveEvaluationObjectsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all active evaluation objects");

                var objects = _unitOfWork.EvaluationObjects.GetAll()
                    .Where(e => e.IsActive == 1)
                    .Select(e => new EvaluationObjectDto
                    {
                        Code = e.Code,
                        Name = e.Name
                    })
                    .ToList();

                _logger.LogInformation("Found {Count} active evaluation objects", objects.Count);
                return objects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActiveEvaluationObjectsAsync");
                throw;
            }
        }

        public async Task<CriteriaSetDetailDto> GetDetailAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting criteria set detail by id: {Id}", id);
                var criteriaSet = _unitOfWork.CriteriaSets.FindFirst(c => c.Id == id);
                if (criteriaSet == null)
                {
                    _logger.LogWarning("Criteria set not found: {Id}", id);
                    return null;
                }

                // Get criteria items
                var criterias = _unitOfWork.Criterias.GetAll()
                    .Where(c => c.CriteriaSetId == id)
                    .Select(c => new CriteriaDto
                    {
                        CriteriaSetId = c.CriteriaSetId,
                        VirtualCode = c.VirtualCode,
                        DisplayCode = c.DisplayCode,
                        Content = c.Content,
                        MaxScore = c.MaxScore,
                        ScoreType = c.ScoreType,
                        VirtualParentCode = c.VirtualParentCode,
                        IsActive = c.IsActive
                    }).ToList();

                // Get classifications sorted by MinScore
                var classifications = _unitOfWork.Classifications.GetAll()
                    .Where(c => c.CriteriaSetId == id)
                    .OrderBy(c => c.MinScore ?? 0)
                    .Select(c => new ClassificationDto
                    {
                        CriteriaSetId = c.CriteriaSetId,
                        Code = c.Code,
                        VirtualId = c.VirtualId,
                        Name = c.Name,
                        Abbreviation = c.Abbreviation,
                        MinScore = c.MinScore ?? 0,
                        MaxScore = c.MaxScore ?? 0,
                        IsActive = c.IsActive
                    }).ToList();

                // Get object codes
                var objectCodes = _unitOfWork.CriteriaSetObjects.GetAll()
                    .Where(cso => cso.CriteriaSetId == id)
                    .Join(_unitOfWork.EvaluationObjects.GetAll(),
                        cso => cso.EvaluationObjectId,
                        eo => eo.Id,
                        (cso, eo) => eo.Code)
                    .ToList();

                return new CriteriaSetDetailDto
                {
                    Id = criteriaSet.Id,
                    Name = criteriaSet.Name,
                    ApplicableYears = criteriaSet.ApplicableYears,
                    ApplicableMonths = criteriaSet.ApplicableMonths,
                    IsActive = criteriaSet.IsActive,
                    Criteria = criterias,
                    Classifications = classifications,
                    ObjectCodes = objectCodes
                };            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetailAsync");
                throw;
            }
        }

        public async Task<CriteriaSetDetailDto> UpdateDetailAsync(Guid id, UpdateCriteriaSetDetailRequest request)
        {
            try
            {
                _logger.LogInformation("Updating criteria set detail with id: {Id}", id);

                using (var transaction = await _unitOfWork.BeginTransactionAsync() as Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
                {
                    try
                    {
                        // Validate đủ thông tin
                        if (string.IsNullOrWhiteSpace(request.Name))
                            throw new Exception("Vui lòng nhập tên bộ tiêu chí");
                        if (request.ObjectCodes == null || !request.ObjectCodes.Any())
                            throw new Exception("Vui lòng chọn ít nhất một đối tượng đánh giá");
                        if (string.IsNullOrWhiteSpace(request.ApplicableMonths))
                            throw new Exception("Vui lòng chọn tháng áp dụng");
                        if (string.IsNullOrWhiteSpace(request.ApplicableYears))
                            throw new Exception("Vui lòng chọn năm áp dụng");
                        if (request.Criteria == null || !request.Criteria.Any())
                            throw new Exception("Vui lòng thêm ít nhất một tiêu chí");
                        if (request.Classifications == null || !request.Classifications.Any())
                            throw new Exception("Vui lòng thêm ít nhất một xếp loại");

                        var criteriaSet = _unitOfWork.CriteriaSets.FindFirst(c => c.Id == id);
                        if (criteriaSet == null)
                            throw new Exception($"Không tìm thấy bộ tiêu chí với id {id}");

                        // Check trùng tên (bỏ qua chính nó)
                        var nameExists = _unitOfWork.CriteriaSets
                            .Find(cs => cs.Name == request.Name && cs.Id != id && cs.IsActive == 1)
                            .Any();
                        if (nameExists)
                            throw new Exception($"Bộ tiêu chí \"{request.Name}\" đã tồn tại");

                        // Check trùng object + tháng + năm (bỏ qua chính nó)
                        if (request.ObjectCodes != null && request.ObjectCodes.Count > 0)
                        {
                            var newMonths = (request.ApplicableMonths ?? "")
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(m => int.TryParse(m.Trim(), out var v) ? v : -1)
                                .Where(v => v > 0).ToHashSet();

                            var newYears = (request.ApplicableYears ?? "")
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(y => int.TryParse(y.Trim(), out var v) ? v : -1)
                                .Where(v => v > 0).ToHashSet();

                            var conflictingSetIds = _unitOfWork.CriteriaSetObjects
                                .Find(cso => cso.CriteriaSetId != id)
                                .Join(_unitOfWork.EvaluationObjects.GetAll().Where(eo => request.ObjectCodes.Contains(eo.Code)),
                                    cso => cso.EvaluationObjectId,
                                    eo => eo.Id,
                                    (cso, eo) => cso.CriteriaSetId)
                                .Distinct()
                                .ToList();

                            if (conflictingSetIds.Any())
                            {
                                var conflictingSets = _unitOfWork.CriteriaSets
                                    .Find(cs => conflictingSetIds.Contains(cs.Id) && cs.IsActive == 1)
                                    .ToList();

                                foreach (var existing in conflictingSets)
                                {
                                    var existingMonths = (existing.ApplicableMonths ?? "")
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(m => int.TryParse(m.Trim(), out var v) ? v : -1)
                                        .Where(v => v > 0).ToHashSet();

                                    var existingYears = (existing.ApplicableYears ?? "")
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(y => int.TryParse(y.Trim(), out var v) ? v : -1)
                                        .Where(v => v > 0).ToHashSet();

                                    bool monthOverlap = !newMonths.Any() || !existingMonths.Any() || newMonths.Overlaps(existingMonths);
                                    bool yearOverlap = !newYears.Any() || !existingYears.Any() || newYears.Overlaps(existingYears);

                                    if (monthOverlap && yearOverlap)
                                    {
                                        var sharedObjects = _unitOfWork.CriteriaSetObjects
                                            .Find(cso => cso.CriteriaSetId == existing.Id)
                                            .Join(_unitOfWork.EvaluationObjects.GetAll().Where(eo => request.ObjectCodes.Contains(eo.Code)),
                                                cso => cso.EvaluationObjectId,
                                                eo => eo.Id,
                                                (cso, eo) => eo.Code)
                                            .ToList();

                                        var overlapMonths = newMonths.Intersect(existingMonths).OrderBy(m => m);
                                        var overlapYears = newYears.Intersect(existingYears).OrderBy(y => y);

                                        throw new Exception(
                                            $"Bộ tiêu chí \"{existing.Name}\" đã áp dụng cho đối tượng [{string.Join(", ", sharedObjects)}] " +
                                            $"vào tháng [{string.Join(", ", overlapMonths)}] năm [{string.Join(", ", overlapYears)}]");
                                    }
                                }
                            }
                        }

                        criteriaSet.Name = request.Name;
                        criteriaSet.ApplicableYears = request.ApplicableYears;
                        criteriaSet.ApplicableMonths = request.ApplicableMonths;
                        _unitOfWork.CriteriaSets.Update(criteriaSet);

                        // Replace object codes
                        var existingObjects = _unitOfWork.CriteriaSetObjects.GetAll()
                            .Where(cso => cso.CriteriaSetId == id).ToList();
                        foreach (var obj in existingObjects)
                            _unitOfWork.CriteriaSetObjects.Remove(obj);

                        if (request.ObjectCodes != null)
                        {
                            var evalObjects = _unitOfWork.EvaluationObjects.GetAll()
                                .Where(eo => request.ObjectCodes.Contains(eo.Code))
                                .ToDictionary(eo => eo.Code, eo => eo.Id);
                            foreach (var code in request.ObjectCodes)
                            {
                                if (evalObjects.TryGetValue(code, out var objId))
                                    _unitOfWork.CriteriaSetObjects.Add(new CriteriaSetObject { CriteriaSetId = id, EvaluationObjectId = objId });
                            }
                        }

                        // Replace criteria
                        var existingCriteria = _unitOfWork.Criterias.GetAll()
                            .Where(c => c.CriteriaSetId == id).ToList();
                        foreach (var c in existingCriteria)
                            _unitOfWork.Criterias.Remove(c);

                        // Replace classifications
                        var existingClassifications = _unitOfWork.Classifications.GetAll()
                            .Where(c => c.CriteriaSetId == id).ToList();
                        foreach (var c in existingClassifications)
                            _unitOfWork.Classifications.Remove(c);

                        await _unitOfWork.SaveChangesAsync();

                        if (request.Criteria != null)
                        {
                            for (int i = 0; i < request.Criteria.Count; i++)
                                AddCriteriaFromTree(id, request.Criteria[i], null, i + 1);
                        }

                        if (request.Classifications != null)
                        {
                            foreach (var cl in request.Classifications)
                            {
                                _unitOfWork.Classifications.Add(new Classification
                                {
                                    CriteriaSetId = id,
                                    Code = cl.Code,
                                    VirtualId = cl.VirtualId,
                                    Name = cl.Name,
                                    Abbreviation = cl.Abbreviation,
                                    MinScore = cl.MinScore,
                                    MaxScore = cl.MaxScore,
                                    IsActive = 1
                                });
                            }
                        }

                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();

                        return new CriteriaSetDetailDto
                        {
                            Id = criteriaSet.Id,
                            Name = criteriaSet.Name,
                            ApplicableYears = criteriaSet.ApplicableYears,
                            ApplicableMonths = criteriaSet.ApplicableMonths,
                            IsActive = criteriaSet.IsActive,
                            ObjectCodes = request.ObjectCodes ?? new List<string>()
                        };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error in UpdateDetailAsync, rolling back transaction");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDetailAsync");
                throw;
            }
        }

        private void AddCriteriaFromTree(Guid criteriaSetId, CreateTreeNodeRequest treeNode, string parentCode, int index = 1)
        {
            // Generate VirtualCode based on tree structure: 1, 1.1, 1.2, 1.2.1, etc.
            string virtualCode = string.IsNullOrEmpty(parentCode)
                ? index.ToString()
                : $"{parentCode}.{index}";

            var criteria = new Criteria
            {
                CriteriaSetId = criteriaSetId,
                VirtualCode = virtualCode,
                DisplayCode = treeNode.DisplayCode,
                Content = treeNode.Content,
                MaxScore = treeNode.MaxScore,
                ScoreType = treeNode.ScoreType,
                VirtualParentCode = parentCode ?? string.Empty,
                IsActive = 1
            };

            _unitOfWork.Criterias.Add(criteria);

            // Recursively add children
            if (treeNode.Children != null && treeNode.Children.Count > 0)
            {
                for (int i = 0; i < treeNode.Children.Count; i++)
                {
                    AddCriteriaFromTree(criteriaSetId, treeNode.Children[i], virtualCode, i + 1);
                }
            }
        }
    }
}
