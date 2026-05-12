using reviewApi.DTO.Page.Configuration;
using reviewApi.Models;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Service.Repositories.Page.Configuration
{
    public class EvaluationFlowDetailService : IEvaluationFlowDetailService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvaluationFlowDetailService> _logger;

        private const string DataNotSyncedError = "Dữ liệu chưa được cập nhật, vui lòng báo lên admin.";

        public EvaluationFlowDetailService(IUnitOfWork unitOfWork, ILogger<EvaluationFlowDetailService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EvaluationFlowDetailDto> GetDetailAsync(string code)
        {
            try
            {
                var flow = _unitOfWork.EvaluationFlows.FindFirst(f => f.FlowCode == code);
                if (flow == null) return null;

                var deptIds = _unitOfWork.EvaluationFlowDepartments.GetAll()
                    .Where(d => d.FlowId == flow.Id)
                    .Select(d => d.DepartmentId)
                    .ToList();
                var departments = _unitOfWork.Departments.GetAll()
                    .Where(d => deptIds.Contains(d.Id))
                    .Select(d => new DepartmentRequest { Code = d.DepartmentCode, Name = d.DepartmentName })
                    .ToList();

                var criteria = _unitOfWork.EvaluationFlowCriterias.GetAll()
                    .Where(c => c.FlowId == flow.Id)
                    .Select(c => c.CriteriaSetId.ToString())
                    .ToList();

                var rolesList = _unitOfWork.EvaluationFlowRoles
                    .FindWithInclude(r => r.FlowId == flow.Id, r => r.Role)
                    .ToList();
                var roles = BuildRoleTree(flow.Id, rolesList);

                var objectsList = _unitOfWork.EvaluationFlowObjects
                    .FindWithInclude(o => o.FlowId == flow.Id, o => o.EvaluationObject)
                    .ToList();
                var objects = BuildObjectTree(flow.Id, objectsList);

                return new EvaluationFlowDetailDto
                {
                    Code = flow.FlowCode,
                    Name = flow.FlowName,
                    Departments = departments,
                    Roles = roles,
                    Objects = objects,
                    Criteria = criteria,
                    IsActive = flow.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetailAsync");
                throw;
            }
        }

        private List<RoleNodeDto> BuildRoleTree(Guid flowId, List<EvaluationFlowRole> roles)
        {
            var rootRoles = roles.Where(r => string.IsNullOrWhiteSpace(r.VirtualParentCode) || r.VirtualParentCode == " ").ToList();
            return rootRoles.Select(role => new RoleNodeDto
            {
                Id = flowId + "_" + role.VirtualCode,
                Code = role.VirtualCode,
                RoleCode = role.Role?.RoleCode ?? role.RoleId.ToString(),
                Name = role.Role?.RoleName ?? role.RoleId.ToString(),
                Children = BuildRoleChildren(flowId, role.VirtualCode, roles)
            }).ToList();
        }

        private List<RoleNodeDto> BuildRoleChildren(Guid flowId, string parentCode, List<EvaluationFlowRole> roles)
        {
            return roles.Where(r => r.VirtualParentCode == parentCode).Select(child => new RoleNodeDto
            {
                Id = flowId + "_" + child.VirtualCode,
                Code = child.VirtualCode,
                RoleCode = child.Role?.RoleCode ?? child.RoleId.ToString(),
                Name = child.Role?.RoleName ?? child.RoleId.ToString(),
                Children = BuildRoleChildren(flowId, child.VirtualCode, roles)
            }).ToList();
        }

        private List<ObjectNodeDto> BuildObjectTree(Guid flowId, List<EvaluationFlowObject> objects)
        {
            var rootObjects = objects.Where(o => string.IsNullOrWhiteSpace(o.VirtualParentCode) || o.VirtualParentCode == " ").ToList();
            return rootObjects.Select(obj => new ObjectNodeDto
            {
                Id = flowId + "_" + obj.VirtualCode,
                Code = obj.VirtualCode,
                ObjectCode = obj.EvaluationObject?.Code ?? obj.EvaluationObjectId.ToString(),
                Name = obj.EvaluationObject?.Name ?? obj.EvaluationObjectId.ToString(),
                Children = BuildObjectChildren(flowId, obj.VirtualCode, objects)
            }).ToList();
        }

        private List<ObjectNodeDto> BuildObjectChildren(Guid flowId, string parentCode, List<EvaluationFlowObject> objects)
        {
            return objects.Where(o => o.VirtualParentCode == parentCode).Select(child => new ObjectNodeDto
            {
                Id = flowId + "_" + child.VirtualCode,
                Code = child.VirtualCode,
                ObjectCode = child.EvaluationObject?.Code ?? child.EvaluationObjectId.ToString(),
                Name = child.EvaluationObject?.Name ?? child.EvaluationObjectId.ToString(),
                Children = BuildObjectChildren(flowId, child.VirtualCode, objects)
            }).ToList();
        }

        public async Task<EvaluationFlowDetailDto> CreateDetailAsync(CreateEvaluationFlowDetailRequest request)
        {
            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync() as Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction;
                try
                {
                    var existing = _unitOfWork.EvaluationFlows.FindFirst(f => f.FlowCode == request.Code);
                    if (existing != null)
                        throw new Exception($"Evaluation flow with code {request.Code} already exists");

                    var flow = new EvaluationFlow { FlowCode = request.Code, FlowName = request.Name, IsActive = 1 };
                    _unitOfWork.EvaluationFlows.Add(flow);
                    await _unitOfWork.SaveChangesAsync();

                    // Save departments
                    if (request.Departments?.Count > 0)
                    {
                        var deptMap = _unitOfWork.Departments.GetAll()
                            .Where(d => request.Departments.Contains(d.DepartmentCode))
                            .ToDictionary(d => d.DepartmentCode, d => d.Id);

                        var missing = request.Departments.Where(c => !deptMap.ContainsKey(c)).ToList();
                        if (missing.Count > 0)
                            throw new Exception(DataNotSyncedError);

                        foreach (var deptCode in request.Departments)
                            _unitOfWork.EvaluationFlowDepartments.Add(new EvaluationFlowDepartment { FlowId = flow.Id, DepartmentId = deptMap[deptCode] });
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Save roles
                    if (request.Roles?.Count > 0)
                    {
                        var roleMap = _unitOfWork.Roles.GetAll().ToDictionary(r => r.RoleCode, r => r.Id);
                        if (!roleMap.Any())
                            throw new Exception(DataNotSyncedError);
                        SaveRoles(flow.Id, request.Roles, roleMap);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Save objects
                    if (request.Objects?.Count > 0)
                    {
                        var objMap = _unitOfWork.EvaluationObjects.GetAll().ToDictionary(o => o.Code, o => o.Id);
                        SaveObjects(flow.Id, request.Objects, objMap);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Save criteria
                    if (request.Criteria?.Count > 0)
                    {
                        foreach (var criteriaId in request.Criteria)
                            if (Guid.TryParse(criteriaId, out Guid id))
                                _unitOfWork.EvaluationFlowCriterias.Add(new EvaluationFlowCriteria { FlowId = flow.Id, CriteriaSetId = id });
                        await _unitOfWork.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return new EvaluationFlowDetailDto
                    {
                        Code = flow.FlowCode,
                        Name = flow.FlowName,
                        Departments = ResolveDepartments(request.Departments),
                        Roles = request.Roles,
                        Objects = request.Objects,
                        Criteria = request.Criteria,
                        IsActive = flow.IsActive
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateDetailAsync");
                throw;
            }
        }

        public async Task<EvaluationFlowDetailDto> UpdateDetailAsync(string code, UpdateEvaluationFlowDetailRequest request)
        {
            try
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync() as Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction;
                try
                {
                    var flow = _unitOfWork.EvaluationFlows.FindFirst(f => f.FlowCode == code)
                        ?? throw new Exception($"Evaluation flow with code {code} not found");

                    flow.FlowName = request.Name;
                    flow.IsActive = request.IsActive;
                    _unitOfWork.EvaluationFlows.Update(flow);
                    await _unitOfWork.SaveChangesAsync();

                    // Replace departments
                    var existingDepts = _unitOfWork.EvaluationFlowDepartments.GetAll().Where(d => d.FlowId == flow.Id).ToList();
                    foreach (var d in existingDepts) _unitOfWork.EvaluationFlowDepartments.Remove(d);
                    await _unitOfWork.SaveChangesAsync();

                    if (request.Departments?.Count > 0)
                    {
                        var deptMap = _unitOfWork.Departments.GetAll()
                            .Where(d => request.Departments.Contains(d.DepartmentCode))
                            .ToDictionary(d => d.DepartmentCode, d => d.Id);

                        var missing = request.Departments.Where(c => !deptMap.ContainsKey(c)).ToList();
                        if (missing.Count > 0)
                            throw new Exception(DataNotSyncedError);

                        foreach (var deptCode in request.Departments)
                            _unitOfWork.EvaluationFlowDepartments.Add(new EvaluationFlowDepartment { FlowId = flow.Id, DepartmentId = deptMap[deptCode] });
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Replace roles
                    var existingRoles = _unitOfWork.EvaluationFlowRoles.GetAll().Where(r => r.FlowId == flow.Id).ToList();
                    foreach (var r in existingRoles) _unitOfWork.EvaluationFlowRoles.Remove(r);
                    await _unitOfWork.SaveChangesAsync();

                    if (request.Roles?.Count > 0)
                    {
                        var roleMap = _unitOfWork.Roles.GetAll().ToDictionary(r => r.RoleCode, r => r.Id);
                        if (!roleMap.Any())
                            throw new Exception(DataNotSyncedError);
                        SaveRoles(flow.Id, request.Roles, roleMap);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Replace objects
                    var existingObjects = _unitOfWork.EvaluationFlowObjects.GetAll().Where(o => o.FlowId == flow.Id).ToList();
                    foreach (var o in existingObjects) _unitOfWork.EvaluationFlowObjects.Remove(o);
                    await _unitOfWork.SaveChangesAsync();

                    if (request.Objects?.Count > 0)
                    {
                        var objMap = _unitOfWork.EvaluationObjects.GetAll().ToDictionary(o => o.Code, o => o.Id);
                        SaveObjects(flow.Id, request.Objects, objMap);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    // Replace criteria
                    var existingCriteria = _unitOfWork.EvaluationFlowCriterias.GetAll().Where(c => c.FlowId == flow.Id).ToList();
                    foreach (var c in existingCriteria) _unitOfWork.EvaluationFlowCriterias.Remove(c);
                    await _unitOfWork.SaveChangesAsync();

                    if (request.Criteria?.Count > 0)
                    {
                        foreach (var criteriaId in request.Criteria)
                            if (Guid.TryParse(criteriaId, out Guid id))
                                _unitOfWork.EvaluationFlowCriterias.Add(new EvaluationFlowCriteria { FlowId = flow.Id, CriteriaSetId = id });
                        await _unitOfWork.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    return new EvaluationFlowDetailDto
                    {
                        Code = flow.FlowCode,
                        Name = flow.FlowName,
                        Departments = ResolveDepartments(request.Departments),
                        Roles = request.Roles,
                        Objects = request.Objects,
                        Criteria = request.Criteria,
                        IsActive = flow.IsActive
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDetailAsync");
                throw;
            }
        }

        private List<DepartmentRequest> ResolveDepartments(List<string> codes)
        {
            if (codes == null || codes.Count == 0) return new List<DepartmentRequest>();
            return _unitOfWork.Departments.GetAll()
                .Where(d => codes.Contains(d.DepartmentCode))
                .Select(d => new DepartmentRequest { Code = d.DepartmentCode, Name = d.DepartmentName })
                .ToList();
        }

        private void SaveRoles(Guid flowId, List<RoleNodeDto> roles, Dictionary<string, Guid> roleMap, string parentCode = null)
        {
            foreach (var role in roles)
            {
                if (roleMap.TryGetValue(role.RoleCode, out var roleId))
                    _unitOfWork.EvaluationFlowRoles.Add(new EvaluationFlowRole
                    {
                        FlowId = flowId,
                        VirtualCode = role.Code,
                        RoleId = roleId,
                        VirtualParentCode = parentCode ?? " "
                    });
                if (role.Children?.Count > 0)
                    SaveRoles(flowId, role.Children, roleMap, role.Code);
            }
        }

        private void SaveObjects(Guid flowId, List<ObjectNodeDto> objects, Dictionary<string, Guid> objMap, string parentCode = null)
        {
            foreach (var obj in objects)
            {
                if (objMap.TryGetValue(obj.ObjectCode, out var objId))
                    _unitOfWork.EvaluationFlowObjects.Add(new EvaluationFlowObject
                    {
                        FlowId = flowId,
                        VirtualCode = obj.Code,
                        EvaluationObjectId = objId,
                        VirtualParentCode = parentCode ?? " "
                    });
                if (obj.Children?.Count > 0)
                    SaveObjects(flowId, obj.Children, objMap, obj.Code);
            }
        }
    }
}
