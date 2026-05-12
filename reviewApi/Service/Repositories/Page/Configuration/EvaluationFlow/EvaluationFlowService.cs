using reviewApi.DTO.Page.Configuration;
using reviewApi.Models;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Service.Repositories.Page.Configuration
{
    public class EvaluationFlowService : IEvaluationFlowService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvaluationFlowService> _logger;

        public EvaluationFlowService(IUnitOfWork unitOfWork, ILogger<EvaluationFlowService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PaginatedResponse<EvaluationFlowDto>> GetAllAsync(string search = null, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting all evaluation flows with search: {Search}, page: {Page}, pageSize: {PageSize}", search, page, pageSize);
                
                var query = _unitOfWork.EvaluationFlows.GetAll().AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(f => f.FlowCode.ToLower().Contains(search) || f.FlowName.ToLower().Contains(search)).AsQueryable();
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Apply pagination
                var skip = (page - 1) * pageSize;
                var dtos = query.Skip(skip).Take(pageSize).ToList().Select(f => {
                    var deptNames = _unitOfWork.EvaluationFlowDepartments.GetAll()
                        .Where(d => d.FlowId == f.Id)
                        .Join(_unitOfWork.Departments.GetAll(),
                            d => d.DepartmentId,
                            dept => dept.Id,
                            (d, dept) => dept.DepartmentName)
                        .ToList();
                    return new EvaluationFlowDto
                    {
                        Code = f.FlowCode,
                        Name = f.FlowName,
                        DepartmentCode = string.Join(", ", deptNames),
                        IsActive = f.IsActive
                    };
                }).ToList();

                return new PaginatedResponse<EvaluationFlowDto>
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

        public async Task<EvaluationFlowDto> GetByCodeAsync(string code)
        {
            try
            {
                _logger.LogInformation("Getting evaluation flow by code: {Code}", code);
                var flow = _unitOfWork.EvaluationFlows.FindFirst(f => f.FlowCode == code);
                if (flow == null)
                {
                    _logger.LogWarning("Evaluation flow not found: {Code}", code);
                    return null;
                }

                var deptNames = _unitOfWork.EvaluationFlowDepartments.GetAll()
                    .Where(d => d.FlowId == flow.Id)
                    .Join(_unitOfWork.Departments.GetAll(),
                        d => d.DepartmentId,
                        dept => dept.Id,
                        (d, dept) => dept.DepartmentName)
                    .ToList();

                return new EvaluationFlowDto
                {
                    Code = flow.FlowCode,
                    Name = flow.FlowName,
                    DepartmentCode = string.Join(", ", deptNames),
                    IsActive = flow.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCodeAsync");
                throw;
            }
        }

        public async Task<EvaluationFlowDto> CreateAsync(CreateEvaluationFlowRequest request)
        {
            try
            {
                _logger.LogInformation("Creating evaluation flow: {Code}", request.Code);

                // Check if already exists
                var existing = _unitOfWork.EvaluationFlows.FindFirst(f => f.FlowCode == request.Code);
                if (existing != null)
                {
                    throw new Exception($"Evaluation flow with code {request.Code} already exists");
                }

                var flow = new EvaluationFlow
                {
                    FlowCode = request.Code,
                    FlowName = request.Name,
                    IsActive = 1
                };

                _unitOfWork.EvaluationFlows.Add(flow);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Evaluation flow created successfully: {Code}", request.Code);

                return new EvaluationFlowDto
                {
                    Code = flow.FlowCode,
                    Name = flow.FlowName,
                    DepartmentCode = "",
                    IsActive = flow.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAsync");
                throw;
            }
        }

        public async Task<EvaluationFlowDto> UpdateAsync(string code, UpdateEvaluationFlowRequest request)
        {
            try
            {
                _logger.LogInformation("Updating evaluation flow: {Code}", code);

                var flow = _unitOfWork.EvaluationFlows.FindFirst(f => f.FlowCode == code);
                if (flow == null)
                {
                    throw new Exception($"Evaluation flow with code {code} not found");
                }

                flow.FlowName = request.Name;
                flow.IsActive = request.IsActive;

                _unitOfWork.EvaluationFlows.Update(flow);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Evaluation flow updated successfully: {Code}", code);

                return new EvaluationFlowDto
                {
                    Code = flow.FlowCode,
                    Name = flow.FlowName,
                    DepartmentCode = "",
                    IsActive = flow.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAsync");
                throw;
            }
        }

        public async Task DeleteAsync(string code)
        {
            try
            {
                _logger.LogInformation("Deleting evaluation flow: {Code}", code);

                var flow = _unitOfWork.EvaluationFlows.FindFirst(f => f.FlowCode == code);
                if (flow == null)
                    throw new Exception($"Evaluation flow with code {code} not found");

                DeleteRelatedRecords(new List<string> { code });

                _unitOfWork.EvaluationFlows.Remove(flow);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Evaluation flow deleted successfully: {Code}", code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAsync");
                throw;
            }
        }

        public async Task DeleteManyAsync(DeleteManyEvaluationFlowRequest request)
        {
            try
            {
                var query = _unitOfWork.EvaluationFlows.GetAll().AsQueryable();

                if (request.IsAll)
                {
                    if (!string.IsNullOrWhiteSpace(request.Filter?.Search))
                    {
                        var s = request.Filter.Search.ToLower();
                        query = query.Where(f => f.FlowCode.ToLower().Contains(s) || f.FlowName.ToLower().Contains(s));
                    }
                    if (request.ExcludeIds?.Count > 0)
                        query = query.Where(f => !request.ExcludeIds.Contains(f.FlowCode));
                }
                else
                {
                    query = query.Where(f => request.IncludeIds.Contains(f.FlowCode));
                }

                var codes = query.Select(f => f.FlowCode).ToList();
                _logger.LogInformation("DeleteMany: deleting {Count} evaluation flows", codes.Count);

                DeleteRelatedRecords(codes);

                var flows = _unitOfWork.EvaluationFlows.GetAll()
                    .Where(f => codes.Contains(f.FlowCode)).ToList();
                foreach (var flow in flows)
                    _unitOfWork.EvaluationFlows.Remove(flow);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteManyAsync");
                throw;
            }
        }

        private void DeleteRelatedRecords(List<string> flowCodes)
        {
            // Resolve FlowCode → FlowId
            var flowIds = _unitOfWork.EvaluationFlows.GetAll()
                .Where(f => flowCodes.Contains(f.FlowCode))
                .Select(f => f.Id)
                .ToList();

            var flowDepartments = _unitOfWork.EvaluationFlowDepartments.GetAll()
                .Where(d => flowIds.Contains(d.FlowId)).ToList();
            foreach (var d in flowDepartments)
                _unitOfWork.EvaluationFlowDepartments.Remove(d);

            var flowRoles = _unitOfWork.EvaluationFlowRoles.GetAll()
                .Where(r => flowIds.Contains(r.FlowId)).ToList();
            foreach (var r in flowRoles)
                _unitOfWork.EvaluationFlowRoles.Remove(r);

            var flowObjects = _unitOfWork.EvaluationFlowObjects.GetAll()
                .Where(o => flowIds.Contains(o.FlowId)).ToList();
            foreach (var o in flowObjects)
                _unitOfWork.EvaluationFlowObjects.Remove(o);

            var flowCriterias = _unitOfWork.EvaluationFlowCriterias.GetAll()
                .Where(c => flowIds.Contains(c.FlowId)).ToList();
            foreach (var c in flowCriterias)
                _unitOfWork.EvaluationFlowCriterias.Remove(c);
        }
    }
}
