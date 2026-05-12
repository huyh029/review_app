using reviewApi.DTO.Page.Configuration;
using reviewApi.Models;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Service.Repositories.Page.Configuration
{
    public class EvaluationObjectRoleService : IEvaluationObjectRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvaluationObjectRoleService> _logger;

        public EvaluationObjectRoleService(IUnitOfWork unitOfWork, ILogger<EvaluationObjectRoleService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private string ResolveObjectCode(Guid id)
            => _unitOfWork.EvaluationObjects.FindFirst(o => o.Id == id)?.Code ?? id.ToString();

        private string ResolveRoleName(User user)
        {
            if (user?.RoleId == null) return "";
            return _unitOfWork.Roles.FindFirst(r => r.Id == user.RoleId)?.RoleName ?? "";
        }

        public async Task<PaginatedResponse<EvaluationObjectRoleDto>> GetAllAsync(string evaluationObjectCode, string search = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var evalObj = _unitOfWork.EvaluationObjects.FindFirst(o => o.Code == evaluationObjectCode);
                if (evalObj == null)
                    return new PaginatedResponse<EvaluationObjectRoleDto> { Data = new List<EvaluationObjectRoleDto>() };

                var query = _unitOfWork.EvaluationObjectRoles.GetAll()
                    .Where(r => r.EvaluationObjectId == evalObj.Id).AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(r => r.User.FullName.ToLower().Contains(search)).AsQueryable();
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var skip = (page - 1) * pageSize;
                var dtos = query.Skip(skip).Take(pageSize).Select(r => new EvaluationObjectRoleDto
                {
                    Id = r.Id, EvaluationObjectCode = evaluationObjectCode,
                    UserId = r.UserId, UserName = r.User.FullName, RoleName = ResolveRoleName(r.User)
                }).ToList();

                return new PaginatedResponse<EvaluationObjectRoleDto>
                {
                    Data = dtos,
                    Pagination = new PaginationInfo { CurrentPage = page, TotalPages = totalPages, TotalItems = totalItems, ItemsPerPage = pageSize }
                };
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in GetAllAsync"); throw; }
        }

        public async Task<PaginatedResponse<EvaluationObjectRoleDto>> GetAllWithoutCodeAsync(string search = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var query = _unitOfWork.EvaluationObjectRoles.GetAll().AsQueryable();
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(r => r.User.FullName.ToLower().Contains(search)).AsQueryable();
                }
                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
                var skip = (page - 1) * pageSize;
                var evalObjects = _unitOfWork.EvaluationObjects.GetAll().ToDictionary(o => o.Id, o => o.Code);
                var dtos = query.Skip(skip).Take(pageSize).ToList().Select(r => new EvaluationObjectRoleDto
                {
                    Id = r.Id,
                    EvaluationObjectCode = evalObjects.TryGetValue(r.EvaluationObjectId, out var c) ? c : r.EvaluationObjectId.ToString(),
                    UserId = r.UserId, UserName = r.User?.FullName ?? "", RoleName = ResolveRoleName(r.User)
                }).ToList();
                return new PaginatedResponse<EvaluationObjectRoleDto>
                {
                    Data = dtos,
                    Pagination = new PaginationInfo { CurrentPage = page, TotalPages = totalPages, TotalItems = totalItems, ItemsPerPage = pageSize }
                };
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in GetAllWithoutCodeAsync"); throw; }
        }

        public async Task<EvaluationObjectRoleDto> GetByIdAsync(Guid id)
        {
            try
            {
                var role = _unitOfWork.EvaluationObjectRoles.FindFirst(r => r.Id == id);
                if (role == null) return null;
                return new EvaluationObjectRoleDto
                {
                    Id = role.Id, EvaluationObjectCode = ResolveObjectCode(role.EvaluationObjectId),
                    UserId = role.UserId, UserName = role.User?.FullName ?? "", RoleName = ResolveRoleName(role.User)
                };
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in GetByIdAsync"); throw; }
        }

        public async Task<EvaluationObjectRoleDto> CreateAsync(CreateEvaluationObjectRoleRequest request)
        {
            try
            {
                var evalObj = _unitOfWork.EvaluationObjects.FindFirst(o => o.Code == request.EvaluationObjectCode)
                    ?? throw new Exception($"Evaluation object with code {request.EvaluationObjectCode} not found");
                var user = _unitOfWork.Users.FindFirst(u => u.Id == request.UserId)
                    ?? throw new Exception($"User with id {request.UserId} not found");
                var existing = _unitOfWork.EvaluationObjectRoles.FindFirst(r =>
                    r.EvaluationObjectId == evalObj.Id && r.UserId == request.UserId);
                if (existing != null) throw new Exception("Role already exists for this evaluation object and user");
                var role = new EvaluationObjectRole { EvaluationObjectId = evalObj.Id, UserId = request.UserId };
                _unitOfWork.EvaluationObjectRoles.Add(role);
                await _unitOfWork.SaveChangesAsync();
                return new EvaluationObjectRoleDto
                {
                    Id = role.Id, EvaluationObjectCode = request.EvaluationObjectCode,
                    UserId = role.UserId, UserName = user.FullName, RoleName = ResolveRoleName(user)
                };
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in CreateAsync"); throw; }
        }

        public async Task<EvaluationObjectRoleDto> UpdateAsync(Guid id, UpdateEvaluationObjectRoleRequest request)
        {
            try
            {
                var role = _unitOfWork.EvaluationObjectRoles.FindFirst(r => r.Id == id)
                    ?? throw new Exception($"Evaluation object role with id {id} not found");
                var user = _unitOfWork.Users.FindFirst(u => u.Id == request.UserId)
                    ?? throw new Exception($"User with id {request.UserId} not found");
                role.UserId = request.UserId;
                _unitOfWork.EvaluationObjectRoles.Update(role);
                await _unitOfWork.SaveChangesAsync();
                return new EvaluationObjectRoleDto
                {
                    Id = role.Id, EvaluationObjectCode = ResolveObjectCode(role.EvaluationObjectId),
                    UserId = role.UserId, UserName = user.FullName, RoleName = ResolveRoleName(user)
                };
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in UpdateAsync"); throw; }
        }

        public async Task<List<EvaluationObjectRoleDto>> GetActiveAsync()
        {
            try
            {
                var activeObjectIds = _unitOfWork.EvaluationObjects.GetAll().Where(e => e.IsActive == 1).Select(e => e.Id).ToList();
                var evalObjects = _unitOfWork.EvaluationObjects.GetAll().ToDictionary(o => o.Id, o => o.Code);
                return _unitOfWork.EvaluationObjectRoles.GetAll()
                    .Where(r => activeObjectIds.Contains(r.EvaluationObjectId)).ToList()
                    .Select(r => new EvaluationObjectRoleDto
                    {
                        Id = r.Id,
                        EvaluationObjectCode = evalObjects.TryGetValue(r.EvaluationObjectId, out var c) ? c : r.EvaluationObjectId.ToString(),
                        UserId = r.UserId, UserName = r.User?.FullName ?? "", RoleName = ResolveRoleName(r.User)
                    }).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in GetActiveAsync"); throw; }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var role = _unitOfWork.EvaluationObjectRoles.FindFirst(r => r.Id == id)
                    ?? throw new Exception($"Evaluation object role with id {id} not found");
                _unitOfWork.EvaluationObjectRoles.Remove(role);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in DeleteAsync"); throw; }
        }

        public async Task<EvaluationObjectRoleTreeResponse> GetAllAsTreeAsync()
        {
            try
            {
                var departments = _unitOfWork.Departments.GetAll().ToList();
                var roles = _unitOfWork.EvaluationObjectRoles.GetAll().ToList();
                var evaluationObjects = _unitOfWork.EvaluationObjects.GetAll().Where(e => e.IsActive == 1).OrderBy(e => e.Code).ToList();
                var rootDept = departments.FirstOrDefault(d => d.ParentId == null);
                if (rootDept == null)
                    return new EvaluationObjectRoleTreeResponse
                    {
                        Data = new TreeNodeDto { Id = "root", Code = "root", Name = "Root" },
                        Headers = evaluationObjects.Select(e => new EvaluationObjectHeaderDto { Code = e.Code, Name = e.Name }).ToList()
                    };
                var evalObjectMap = evaluationObjects.ToDictionary(o => o.Id, o => o.Code);
                var roleMap = _unitOfWork.Roles.GetAll().ToDictionary(r => r.Id, r => r);
                var allUsers = _unitOfWork.Users.GetAll().ToList();
                var tree = BuildDepartmentTree(rootDept, departments, roles, evalObjectMap, allUsers, roleMap);
                return new EvaluationObjectRoleTreeResponse
                {
                    Data = tree,
                    Headers = evaluationObjects.Select(e => new EvaluationObjectHeaderDto { Code = e.Code, Name = e.Name }).ToList()
                };
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in GetAllAsTreeAsync"); throw; }
        }

        public async Task<EvaluationObjectRoleTreeResponse> SearchAsTreeAsync(string search)
        {
            try
            {
                var departments       = _unitOfWork.Departments.GetAll().ToList();
                var evalObjectRoles   = _unitOfWork.EvaluationObjectRoles.GetAll().ToList();
                var allUsers          = _unitOfWork.Users.GetAll().ToList();
                var evaluationObjects = _unitOfWork.EvaluationObjects.GetAll().Where(e => e.IsActive == 1).OrderBy(e => e.Code).ToList();
                var evalObjectMap     = evaluationObjects.ToDictionary(o => o.Id, o => o.Code);
                var roleEntities      = _unitOfWork.Roles.GetAll().ToList();
                var roleMap           = roleEntities.ToDictionary(r => r.Id, r => r);

                var q = search.ToLower().Trim();

                var matchedDeptIds = departments
                    .Where(d => d.DepartmentName.ToLower().Contains(q) || d.DepartmentCode.ToLower().Contains(q))
                    .Select(d => d.Id).ToHashSet();

                var matchedRoleIds = roleEntities
                    .Where(r => r.RoleName.ToLower().Contains(q) || r.RoleCode.ToLower().Contains(q))
                    .Select(r => r.Id).ToHashSet();

                var matchedUserIds = allUsers
                    .Where(u => u.FullName.ToLower().Contains(q))
                    .Select(u => u.Id).ToHashSet();

                bool isDeptSearch = matchedDeptIds.Count > 0;
                bool isRoleSearch = matchedRoleIds.Count > 0 && matchedUserIds.Count == 0 && !isDeptSearch;
                bool isUserSearch = matchedUserIds.Count > 0 && !isDeptSearch;

                // userId -> priority (0=match trực tiếp, 1=trong dept match)
                var userPriority = new Dictionary<Guid, int>();

                if (isDeptSearch)
                {
                    foreach (var u in allUsers.Where(u => u.DepartmentId.HasValue && matchedDeptIds.Contains(u.DepartmentId.Value)))
                    {
                        int p = matchedUserIds.Contains(u.Id) || (u.RoleId.HasValue && matchedRoleIds.Contains(u.RoleId.Value)) ? 0 : 1;
                        userPriority[u.Id] = p;
                    }
                    foreach (var uid in matchedUserIds.Union(allUsers.Where(u => u.RoleId.HasValue && matchedRoleIds.Contains(u.RoleId.Value)).Select(u => u.Id)))
                        if (!userPriority.ContainsKey(uid)) userPriority[uid] = 0;
                }
                else if (isRoleSearch)
                {
                    foreach (var u in allUsers.Where(u => u.RoleId.HasValue && matchedRoleIds.Contains(u.RoleId.Value)))
                        userPriority[u.Id] = 0;
                }
                else if (isUserSearch)
                {
                    foreach (var uid in matchedUserIds) userPriority[uid] = 0;
                }
                else
                {
                    return new EvaluationObjectRoleTreeResponse
                    {
                        Data    = new TreeNodeDto { Id = "no-result", Code = "no-result", Name = "Không tìm thấy kết quả" },
                        Headers = evaluationObjects.Select(e => new EvaluationObjectHeaderDto { Code = e.Code, Name = e.Name }).ToList()
                    };
                }

                if (userPriority.Count == 0)
                    return new EvaluationObjectRoleTreeResponse
                    {
                        Data    = new TreeNodeDto { Id = "no-result", Code = "no-result", Name = "Không tìm thấy kết quả" },
                        Headers = evaluationObjects.Select(e => new EvaluationObjectHeaderDto { Code = e.Code, Name = e.Name }).ToList()
                    };

                // Build cây từ root — chỉ include nhánh có user
                var deptById = departments.ToDictionary(d => d.Id);
                var requiredDeptIds = new HashSet<Guid>();
                foreach (var uid in userPriority.Keys)
                {
                    var user = allUsers.FirstOrDefault(u => u.Id == uid);
                    if (user?.DepartmentId == null) continue;
                    var deptId = user.DepartmentId.Value;
                    while (deptById.TryGetValue(deptId, out var dept))
                    {
                        requiredDeptIds.Add(deptId);
                        if (dept.ParentId == null) break;
                        deptId = dept.ParentId.Value;
                    }
                }

                var rootDept = departments.FirstOrDefault(d => d.ParentId == null);
                if (rootDept == null)
                    return new EvaluationObjectRoleTreeResponse
                    {
                        Data    = new TreeNodeDto { Id = "root", Code = "root", Name = "Root" },
                        Headers = evaluationObjects.Select(e => new EvaluationObjectHeaderDto { Code = e.Code, Name = e.Name }).ToList()
                    };

                var tree = BuildFilteredTree(rootDept, departments, evalObjectRoles, evalObjectMap,
                    userPriority, allUsers, roleMap, requiredDeptIds);

                return new EvaluationObjectRoleTreeResponse
                {
                    Data    = tree,
                    Headers = evaluationObjects.Select(e => new EvaluationObjectHeaderDto { Code = e.Code, Name = e.Name }).ToList()
                };
            }
            catch (Exception ex) { _logger.LogError(ex, "Error in SearchAsTreeAsync"); throw; }
        }

        private TreeNodeDto BuildFilteredTree(
            Department dept, List<Department> allDepts, List<EvaluationObjectRole> roles,
            Dictionary<Guid, string> evalObjectMap, Dictionary<Guid, int> userPriority,
            List<User> allUsers, Dictionary<Guid, Role> roleMap, HashSet<Guid> requiredDeptIds)
        {
            var node = new TreeNodeDto
            {
                Id = dept.DepartmentCode, Code = dept.DepartmentCode, Name = dept.DepartmentName,
                Children = new List<TreeNodeDto>(), Individuals = new List<IndividualDto>()
            };

            var childDepts = allDepts.Where(d => d.ParentId == dept.Id && requiredDeptIds.Contains(d.Id)).ToList();
            foreach (var child in childDepts)
                node.Children.Add(BuildFilteredTree(child, allDepts, roles, evalObjectMap, userPriority, allUsers, roleMap, requiredDeptIds));

            var deptUsers = allUsers
                .Where(u => u.DepartmentId == dept.Id && userPriority.ContainsKey(u.Id))
                .OrderBy(u => userPriority[u.Id]).ThenBy(u => u.FullName).ToList();

            foreach (var user in deptUsers)
            {
                var userRoles = roles.Where(r => r.UserId == user.Id)
                    .Select(r => evalObjectMap.TryGetValue(r.EvaluationObjectId, out var c) ? c : r.EvaluationObjectId.ToString()).ToList();
                var roleName = user.RoleId.HasValue && roleMap.TryGetValue(user.RoleId.Value, out var role) ? role.RoleName : "";
                node.Individuals.Add(new IndividualDto
                {
                    Id = user.Id, Name = user.FullName, Code = user.Id.ToString(),
                    RoleName = roleName, SelectedObjectIds = userRoles
                });
            }
            return node;
        }

        private void AddChildDepartments(Department parent, List<Department> allDepts, List<Department> result)
        {
            var children = allDepts.Where(d => d.ParentId == parent.Id).ToList();
            foreach (var child in children)
            {
                result.Add(child);
                AddChildDepartments(child, allDepts, result);
            }
        }

        private TreeNodeDto BuildDepartmentTree(Department dept, List<Department> allDepts,
            List<EvaluationObjectRole> roles, Dictionary<Guid, string> evalObjectMap,
            List<User> allUsers, Dictionary<Guid, Role> roleMap)
        {
            var node = new TreeNodeDto
            {
                Id = dept.DepartmentCode, Code = dept.DepartmentCode, Name = dept.DepartmentName,
                Children = new List<TreeNodeDto>(), Individuals = new List<IndividualDto>()
            };

            var childDepts = allDepts.Where(d => d.ParentId == dept.Id).ToList();
            foreach (var childDept in childDepts)
                node.Children.Add(BuildDepartmentTree(childDept, allDepts, roles, evalObjectMap, allUsers, roleMap));

            var deptUsers = allUsers.Where(u => u.DepartmentId == dept.Id).ToList();
            foreach (var user in deptUsers)
            {
                var userRoles = roles.Where(r => r.UserId == user.Id)
                    .Select(r => evalObjectMap.TryGetValue(r.EvaluationObjectId, out var c) ? c : r.EvaluationObjectId.ToString()).ToList();
                var roleName = user.RoleId.HasValue && roleMap.TryGetValue(user.RoleId.Value, out var role) ? role.RoleName : "";
                node.Individuals.Add(new IndividualDto
                {
                    Id = user.Id, Name = user.FullName, Code = user.Id.ToString(),
                    RoleName = roleName, SelectedObjectIds = userRoles
                });
            }
            return node;
        }

        private TreeNodeDto BuildSearchTree(Department dept, List<Department> matchingDepts,
            List<Department> allDepts, List<EvaluationObjectRole> roles, Dictionary<Guid, string> evalObjectMap)
        {
            var node = new TreeNodeDto
            {
                Id = dept.DepartmentCode, Code = dept.DepartmentCode, Name = dept.DepartmentName,
                Children = new List<TreeNodeDto>(), Individuals = new List<IndividualDto>()
            };
            var childDepts = matchingDepts.Where(d => d.ParentId == dept.Id).ToList();
            foreach (var childDept in childDepts)
                node.Children.Add(BuildSearchTree(childDept, matchingDepts, allDepts, roles, evalObjectMap));
            var deptUsers = _unitOfWork.Users.GetAll().Where(u => u.DepartmentId == dept.Id).ToList();
            foreach (var user in deptUsers)
            {
                var userRoles = roles.Where(r => r.UserId == user.Id)
                    .Select(r => evalObjectMap.TryGetValue(r.EvaluationObjectId, out var c) ? c : r.EvaluationObjectId.ToString()).ToList();
                node.Individuals.Add(new IndividualDto
                {
                    Id = user.Id, Name = user.FullName, Code = user.Id.ToString(),
                    RoleName = ResolveRoleName(user), SelectedObjectIds = userRoles
                });
            }
            return node;
        }
    }
}
