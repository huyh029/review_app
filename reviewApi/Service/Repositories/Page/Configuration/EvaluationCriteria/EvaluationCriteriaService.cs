using reviewApi.DTO.Page.Configuration;
using reviewApi.Models;
using reviewApi.Service.Page.Configuration;
using reviewApi.Service.General;

namespace reviewApi.Service.Repositories.Page.Configuration
{
    public class EvaluationCriteriaService : IEvaluationCriteriaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvaluationCriteriaService> _logger;
        private readonly ITreeBuilderService _treeBuilderService;

        public EvaluationCriteriaService(IUnitOfWork unitOfWork, ILogger<EvaluationCriteriaService> logger, ITreeBuilderService treeBuilderService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _treeBuilderService = treeBuilderService;
        }

        public async Task<PaginatedResponse<CriteriaSetDto>> GetAllAsync(string search = null, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting all criteria sets with search: {Search}, page: {Page}, pageSize: {PageSize}", search, page, pageSize);
                
                var query = _unitOfWork.CriteriaSets.GetAll().AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(c => c.Name.ToLower().Contains(search)).AsQueryable();
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Apply pagination
                var skip = (page - 1) * pageSize;
                var dtos = query.Skip(skip).Take(pageSize).Select(c => new CriteriaSetDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ApplicableYears = c.ApplicableYears,
                    ApplicableMonths = c.ApplicableMonths,
                    IsActive = c.IsActive
                }).ToList();

                return new PaginatedResponse<CriteriaSetDto>
                {
                    Data = dtos,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        ItemsPerPage = pageSize
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllAsync");
                throw;
            }
        }

        public async Task<CriteriaSetDto> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting criteria set by id: {Id}", id);
                var criteriaSet = _unitOfWork.CriteriaSets.FindFirst(c => c.Id == id);
                if (criteriaSet == null)
                {
                    _logger.LogWarning("Criteria set not found: {Id}", id);
                    return null;
                }

                return new CriteriaSetDto
                {
                    Id = criteriaSet.Id,
                    Name = criteriaSet.Name,
                    ApplicableYears = criteriaSet.ApplicableYears,
                    ApplicableMonths = criteriaSet.ApplicableMonths,
                    IsActive = criteriaSet.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAsync");
                throw;
            }
        }

        public async Task<CriteriaSetDto> CreateAsync(CreateCriteriaSetRequest request)
        {
            try
            {
                _logger.LogInformation("Creating criteria set: {Name}", request.Name);

                var criteriaSet = new CriteriaSet
                {
                    Name = request.Name,
                    ApplicableYears = request.ApplicableYears,
                    ApplicableMonths = request.ApplicableMonths,
                    IsActive = 1
                };

                _unitOfWork.CriteriaSets.Add(criteriaSet);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Criteria set created successfully: {Id}", criteriaSet.Id);

                return new CriteriaSetDto
                {
                    Id = criteriaSet.Id,
                    Name = criteriaSet.Name,
                    ApplicableYears = criteriaSet.ApplicableYears,
                    ApplicableMonths = criteriaSet.ApplicableMonths,
                    IsActive = criteriaSet.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAsync");
                throw;
            }
        }

        public async Task<CriteriaSetDto> UpdateAsync(Guid id, UpdateCriteriaSetRequest request)
        {
            try
            {
                _logger.LogInformation("Updating criteria set: {Id}", id);

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
                var objectCodes = _unitOfWork.CriteriaSetObjects
                    .Find(cso => cso.CriteriaSetId == id)
                    .Join(_unitOfWork.EvaluationObjects.GetAll(),
                        cso => cso.EvaluationObjectId,
                        eo => eo.Id,
                        (cso, eo) => eo.Code)
                    .ToList();

                if (objectCodes.Any())
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
                        .Join(_unitOfWork.EvaluationObjects.GetAll().Where(eo => objectCodes.Contains(eo.Code)),
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
                                    .Join(_unitOfWork.EvaluationObjects.GetAll().Where(eo => objectCodes.Contains(eo.Code)),
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
                criteriaSet.IsActive = request.IsActive;

                _unitOfWork.CriteriaSets.Update(criteriaSet);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Criteria set updated successfully: {Id}", id);

                return new CriteriaSetDto
                {
                    Id = criteriaSet.Id,
                    Name = criteriaSet.Name,
                    ApplicableYears = criteriaSet.ApplicableYears,
                    ApplicableMonths = criteriaSet.ApplicableMonths,
                    IsActive = criteriaSet.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAsync");
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting criteria set: {Id}", id);

                var criteriaSet = _unitOfWork.CriteriaSets.FindFirst(c => c.Id == id);
                if (criteriaSet == null)
                    throw new Exception($"Criteria set with id {id} not found");

                DeleteRelatedRecords(new List<Guid> { id });

                _unitOfWork.CriteriaSets.Remove(criteriaSet);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Criteria set deleted successfully: {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAsync");
                throw;
            }
        }

        public async Task DeleteManyAsync(DeleteManyCriteriaSetRequest request)
        {
            try
            {
                var query = _unitOfWork.CriteriaSets.GetAll().AsQueryable();

                if (request.IsAll)
                {
                    if (!string.IsNullOrWhiteSpace(request.Filter?.Search))
                    {
                        var s = request.Filter.Search.ToLower();
                        query = query.Where(c => c.Name.ToLower().Contains(s));
                    }
                    if (request.ExcludeIds?.Count > 0)
                        query = query.Where(c => !request.ExcludeIds.Contains(c.Id));
                }
                else
                {
                    query = query.Where(c => request.IncludeIds.Contains(c.Id));
                }

                var ids = query.Select(c => c.Id).ToList();
                _logger.LogInformation("DeleteMany: deleting {Count} criteria sets", ids.Count);

                DeleteRelatedRecords(ids);

                var criteriaSets = _unitOfWork.CriteriaSets.GetAll()
                    .Where(c => ids.Contains(c.Id)).ToList();
                foreach (var cs in criteriaSets)
                    _unitOfWork.CriteriaSets.Remove(cs);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteManyAsync");
                throw;
            }
        }

        private void DeleteRelatedRecords(List<Guid> criteriaSetIds)
        {
            // Delete Evaluations (EvaluationScores and Comments cascade)
            var evaluations = _unitOfWork.Evaluations.GetAll()
                .Where(e => criteriaSetIds.Contains(e.CriteriaSetId))
                .ToList();
            foreach (var e in evaluations)
                _unitOfWork.Evaluations.Remove(e);

            // Delete EvaluationFlowCriterias
            var flowCriterias = _unitOfWork.EvaluationFlowCriterias.GetAll()
                .Where(efc => criteriaSetIds.Contains(efc.CriteriaSetId))
                .ToList();
            foreach (var efc in flowCriterias)
                _unitOfWork.EvaluationFlowCriterias.Remove(efc);

            // Delete ReportTypeCriterias
            var reportTypeCriterias = _unitOfWork.ReportTypeCriterias.GetAll()
                .Where(rtc => criteriaSetIds.Contains(rtc.CriteriaSetId))
                .ToList();
            foreach (var rtc in reportTypeCriterias)
                _unitOfWork.ReportTypeCriterias.Remove(rtc);
        }

        public async Task<CriteriaDto> CreateCriteriaAsync(CreateCriteriaRequest request)
        {
            try
            {
                _logger.LogInformation("Creating criteria for CriteriaSetId: {CriteriaSetId}, VirtualCode: {VirtualCode}", 
                    request.CriteriaSetId, request.VirtualCode);

                // Verify criteria set exists
                var criteriaSet = _unitOfWork.CriteriaSets.FindFirst(cs => cs.Id == request.CriteriaSetId);
                if (criteriaSet == null)
                {
                    throw new Exception($"Criteria set with id {request.CriteriaSetId} not found");
                }

                // Check if criteria with same virtual code already exists
                var existingCriteria = _unitOfWork.Criterias.FindFirst(c => 
                    c.CriteriaSetId == request.CriteriaSetId && c.VirtualCode == request.VirtualCode);
                if (existingCriteria != null)
                {
                    throw new Exception($"Criteria with virtual code {request.VirtualCode} already exists in this criteria set");
                }

                var criteria = new Criteria
                {
                    CriteriaSetId = request.CriteriaSetId,
                    VirtualCode = request.VirtualCode,
                    DisplayCode = request.DisplayCode,
                    Content = request.Content,
                    MaxScore = request.MaxScore,
                    ScoreType = request.ScoreType,
                    VirtualParentCode = request.VirtualParentCode,
                    IsActive = 1
                };

                _unitOfWork.Criterias.Add(criteria);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Criteria created successfully");

                return new CriteriaDto
                {
                    CriteriaSetId = criteria.CriteriaSetId,
                    VirtualCode = criteria.VirtualCode,
                    DisplayCode = criteria.DisplayCode,
                    Content = criteria.Content,
                    MaxScore = criteria.MaxScore,
                    ScoreType = criteria.ScoreType,
                    VirtualParentCode = criteria.VirtualParentCode,
                    IsActive = criteria.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateCriteriaAsync");
                throw;
            }
        }

        public async Task<CriteriaSetDetailDto> GetDetailByIdAsync(Guid id)
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

                // Get classifications
                var classifications = _unitOfWork.Classifications.GetAll()
                    .Where(c => c.CriteriaSetId == id)
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

                return new CriteriaSetDetailDto
                {
                    Id = criteriaSet.Id,
                    Name = criteriaSet.Name,
                    ApplicableYears = criteriaSet.ApplicableYears,
                    ApplicableMonths = criteriaSet.ApplicableMonths,
                    IsActive = criteriaSet.IsActive,
                    Criteria = criterias,
                    Classifications = classifications
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetailByIdAsync");
                throw;
            }
        }

        public async Task<CriteriaSetDto> CreateCriteriaSetDetailAsync(CreateCriteriaSetDetailRequest request)
        {
            try
            {
                _logger.LogInformation("Creating criteria set detail with {CriteriaCount} criteria and {ClassificationCount} classifications", 
                    request.Criteria?.Count ?? 0, request.Classifications?.Count ?? 0);

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

                // Check trùng tên
                var nameExists = _unitOfWork.CriteriaSets
                    .Find(cs => cs.Name == request.Name && cs.IsActive == 1)
                    .Any();
                if (nameExists)
                    throw new Exception($"Bộ tiêu chí \"{request.Name}\" đã tồn tại");

                // Check trùng: cùng objectCode + giao tháng + giao năm
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
                        .Find(cso => true)
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

                // Use transaction to ensure all-or-nothing save
                using (var transaction = await _unitOfWork.BeginTransactionAsync() as Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction)
                {
                    try
                    {
                        // Create criteria set
                        var criteriaSet = new CriteriaSet
                        {
                            Name = request.Name,
                            ApplicableYears = request.ApplicableYears,
                            ApplicableMonths = request.ApplicableMonths,
                            IsActive = 1
                        };

                        _unitOfWork.CriteriaSets.Add(criteriaSet);
                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogInformation("Criteria set created with id: {Id}", criteriaSet.Id);

                        // Create object codes
                        if (request.ObjectCodes != null)
                        {
                            var evalObjects = _unitOfWork.EvaluationObjects.GetAll()
                                .Where(eo => request.ObjectCodes.Contains(eo.Code))
                                .ToDictionary(eo => eo.Code, eo => eo.Id);
                            foreach (var code in request.ObjectCodes)
                            {
                                if (evalObjects.TryGetValue(code, out var objId))
                                    _unitOfWork.CriteriaSetObjects.Add(new CriteriaSetObject { CriteriaSetId = criteriaSet.Id, EvaluationObjectId = objId });
                            }
                        }

                        // Create criteria items from tree structure
                        if (request.Criteria != null && request.Criteria.Count > 0)
                        {
                            for (int i = 0; i < request.Criteria.Count; i++)
                            {
                                AddCriteriaFromTree(criteriaSet.Id, request.Criteria[i], null, i + 1);
                            }
                        }

                        // Create classifications
                        if (request.Classifications != null && request.Classifications.Count > 0)
                        {
                            foreach (var classificationRequest in request.Classifications)
                            {
                                var classification = new Classification
                                {
                                    CriteriaSetId = criteriaSet.Id,
                                    Code = classificationRequest.Code,
                                    VirtualId = classificationRequest.VirtualId,
                                    Name = classificationRequest.Name,
                                    Abbreviation = classificationRequest.Abbreviation,
                                    MinScore = classificationRequest.MinScore,
                                    MaxScore = classificationRequest.MaxScore,
                                    IsActive = 1
                                };

                                _unitOfWork.Classifications.Add(classification);
                            }
                        }

                        // Save all at once
                        await _unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                        _logger.LogInformation("Criteria set detail created successfully");

                        return new CriteriaSetDto
                        {
                            Id = criteriaSet.Id,
                            Name = criteriaSet.Name,
                            ApplicableYears = criteriaSet.ApplicableYears,
                            ApplicableMonths = criteriaSet.ApplicableMonths,
                            IsActive = criteriaSet.IsActive
                        };
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, "Error in CreateCriteriaSetDetailAsync, rolling back transaction");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateCriteriaSetDetailAsync");
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
