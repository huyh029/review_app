using reviewApi.Models;

namespace reviewApi.Service.Repositories
{
    public class SetupService : ISetupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SetupService> _logger;

        public SetupService(IUnitOfWork unitOfWork, ILogger<SetupService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SetBaseDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting SetBaseData...");

                // Add Roles
                var roles = new List<Role>
                {
                    new Role { RoleCode = "ADMIN", RoleName = "Administrator" },
                    new Role { RoleCode = "BO_TRUONG", RoleName = "Bộ trưởng" },
                    new Role { RoleCode = "CAN_BO", RoleName = "Cán bộ" },
                    new Role { RoleCode = "CHANH_VAN_PHONG", RoleName = "Chánh Văn phòng" },
                    new Role { RoleCode = "CHIEN_SI", RoleName = "Chiến sĩ" },
                    new Role { RoleCode = "CUC_TRUONG", RoleName = "Cục trưởng" },
                    new Role { RoleCode = "DOI_TRUONG", RoleName = "Đội trưởng" },
                    new Role { RoleCode = "GIAM_DOC", RoleName = "Giám đốc" },
                    new Role { RoleCode = "GIAM_DOC_CONG", RoleName = "Giám Đốc CTTDT" },
                    new Role { RoleCode = "GIAM_DOC_TTTTCH", RoleName = "Giám đốc TTTTCH" },
                    new Role { RoleCode = "HIEU_TRUONG", RoleName = "Hiệu Trưởng" },
                    new Role { RoleCode = "KE_TOAN", RoleName = "Kế toán" },
                    new Role { RoleCode = "NHAN_VIEN", RoleName = "Nhân viên" },
                    new Role { RoleCode = "PHO_CHANH_VAN_PHONG", RoleName = "Phó Chánh Văn phòng" },
                    new Role { RoleCode = "PHO_CUC_TRUONG", RoleName = "Phó Cục trưởng" },
                    new Role { RoleCode = "PHO_DOI_TRUONG", RoleName = "Phó Đội trưởng" },
                    new Role { RoleCode = "PHO_GIAM_DOC", RoleName = "Phó Giám đốc" },
                    new Role { RoleCode = "PHO_GIAM_DOC_CONG", RoleName = "Phó Giám đốc CTTDT" },
                    new Role { RoleCode = "PHO_GIAM_DOC_TTTTCH", RoleName = "Phó Giám đốc TTTTCH" },
                    new Role { RoleCode = "PHO_HIEU_TRUONG", RoleName = "Phó Hiệu trưởng" },
                    new Role { RoleCode = "PHO_TO_TRUONG", RoleName = "Phó Tổ trưởng" },
                    new Role { RoleCode = "PHO_TONG_CUC_TRUONG", RoleName = "Phó Tổng cục trưởng" },
                    new Role { RoleCode = "PHO_TRUONG_BAN", RoleName = "Phó Trưởng ban" },
                    new Role { RoleCode = "PHO_TRUONG_CONG_AN_XA", RoleName = "Phó Trưởng Công an xã/phường" },
                    new Role { RoleCode = "PHO_TRUONG_PHONG", RoleName = "Phó Trưởng phòng" },
                    new Role { RoleCode = "PHO_TU_LENH", RoleName = "Phó Tư lệnh" },
                    new Role { RoleCode = "PHO_VIEN_TRUONG", RoleName = "Phó Viện Trưởng" },
                    new Role { RoleCode = "PHO_VU_TRUONG", RoleName = "Phó Vụ trưởng" },
                    new Role { RoleCode = "THU_KY", RoleName = "Thư ký" },
                    new Role { RoleCode = "THU_KY_LANH_DAO_BO", RoleName = "Thư ký lãnh đạo bộ" },
                    new Role { RoleCode = "THU_TRUONG", RoleName = "Thứ trưởng" },
                    new Role { RoleCode = "TO_TRUONG", RoleName = "Tổ trưởng" },
                    new Role { RoleCode = "TONG_CUC_TRUONG", RoleName = "Tổng cục trưởng" },
                    new Role { RoleCode = "TRO_LY", RoleName = "Trợ lý" },
                    new Role { RoleCode = "TRUONG_BAN", RoleName = "Trưởng Ban" },
                    new Role { RoleCode = "TRUONG_CONG_AN_XA", RoleName = "Trưởng Công an xã/phường" },
                    new Role { RoleCode = "TRUONG_PHONG", RoleName = "Trưởng phòng" },
                    new Role { RoleCode = "TU_LENH", RoleName = "Tư Lệnh" },
                    new Role { RoleCode = "VAN_THU", RoleName = "Văn thư" },
                    new Role { RoleCode = "VIEN_TRUONG", RoleName = "Viện Trưởng" }
                };

                foreach (var role in roles)
                {
                    if (_unitOfWork.Roles.FindFirst(r => r.RoleCode == role.RoleCode) == null)
                    {
                        _unitOfWork.Roles.Add(role);
                    }
                }

                // Save roles trước để có thể query
                await _unitOfWork.SaveChangesAsync();

                // Add Departments
                // Build departments with GUID-based parent references
                var deptMap = new Dictionary<string, Department>
                {
                    ["G00"]                = new Department { Id = Guid.NewGuid(), DepartmentCode = "G00",                DepartmentName = "Tất cả đơn vị" },
                    ["G01.000.000"]        = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.000.000",        DepartmentName = "Bộ Công an" },
                    ["G01.501.000"]        = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.000",        DepartmentName = "Văn Phòng Bộ Công an" },
                    ["G01.501.001.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.001.000",    DepartmentName = "Phòng 1" },
                    ["G01.501.002.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.002.000",    DepartmentName = "Phòng 2" },
                    ["G01.501.003.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.003.000",    DepartmentName = "Phòng 3" },
                    ["G01.501.004.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.004.000",    DepartmentName = "Phòng 4" },
                    ["G01.501.005.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.005.000",    DepartmentName = "Phòng 5" },
                    ["G01.501.006.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.006.000",    DepartmentName = "Phòng 6" },
                    ["G01.501.007.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.007.000",    DepartmentName = "Phòng 7" },
                    ["G01.501.008.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.008.000",    DepartmentName = "Phòng 8" },
                    ["G01.501.012.000"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.012.000",    DepartmentName = "Trung tâm Thông tin chỉ huy" },
                    ["G01.501.008.0001"]   = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.008.0001",   DepartmentName = "Đội Kế hoạch" },
                    ["G01.501.008.0002"]   = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.008.0002",   DepartmentName = "Đội Kế toán, tài vụ" },
                    ["G01.501.008.0003"]   = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.008.0003",   DepartmentName = "Đội xe" },
                    ["G01.501.008.0005"]   = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.008.0005",   DepartmentName = "Đội Hậu cần phía nam" },
                    ["G01.501.012.001"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.012.001",    DepartmentName = "Ban 1 - TTTTCH" },
                    ["G01.501.012.002"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.012.002",    DepartmentName = "Ban 2 - TTTTCH" },
                    ["G01.501.012.003"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.012.003",    DepartmentName = "Ban 3 - TTTTCH" },
                    ["G01.501.012.004"]    = new Department { Id = Guid.NewGuid(), DepartmentCode = "G01.501.012.004",    DepartmentName = "Ban 4 - TTTTCH" },
                };

                // Assign ParentId by looking up parent GUID
                var parentMapping = new Dictionary<string, string?>
                {
                    ["G00"]                = null,
                    ["G01.000.000"]        = "G00",
                    ["G01.501.000"]        = "G01.000.000",
                    ["G01.501.001.000"]    = "G01.501.000",
                    ["G01.501.002.000"]    = "G01.501.000",
                    ["G01.501.003.000"]    = "G01.501.000",
                    ["G01.501.004.000"]    = "G01.501.000",
                    ["G01.501.005.000"]    = "G01.501.000",
                    ["G01.501.006.000"]    = "G01.501.000",
                    ["G01.501.007.000"]    = "G01.501.000",
                    ["G01.501.008.000"]    = "G01.501.000",
                    ["G01.501.012.000"]    = "G01.501.000",
                    ["G01.501.008.0001"]   = "G01.501.008.000",
                    ["G01.501.008.0002"]   = "G01.501.008.000",
                    ["G01.501.008.0003"]   = "G01.501.008.000",
                    ["G01.501.008.0005"]   = "G01.501.008.000",
                    ["G01.501.012.001"]    = "G01.501.012.000",
                    ["G01.501.012.002"]    = "G01.501.012.000",
                    ["G01.501.012.003"]    = "G01.501.012.000",
                    ["G01.501.012.004"]    = "G01.501.012.000",
                };

                foreach (var kvp in parentMapping)
                {
                    if (kvp.Value != null)
                        deptMap[kvp.Key].ParentId = deptMap[kvp.Value].Id;
                }

                var departments = deptMap.Values.ToList();

                foreach (var dept in departments)
                {
                    if (_unitOfWork.Departments.FindFirst(d => d.DepartmentCode == dept.DepartmentCode) == null)
                    {
                        _unitOfWork.Departments.Add(dept);
                    }
                }

                // Save departments trước để có thể query
                await _unitOfWork.SaveChangesAsync();

                // Add Users — dùng data đã có trong memory (tránh Oracle transaction isolation issue)
                var allRoles = roles.ToDictionary(r => r.RoleCode, r => r.Id);
                var allDepts = deptMap; // đã có sẵn từ bước trên

                var users = new List<User>
                {
                    new User { FullName = "Administrator",             RoleId = allRoles.GetValueOrDefault("ADMIN"),             DepartmentId = allDepts.GetValueOrDefault("G00")?.Id ?? Guid.Empty },
                    new User { FullName = "Cán Bộ Phòng 1",           RoleId = allRoles.GetValueOrDefault("CAN_BO"),            DepartmentId = allDepts.GetValueOrDefault("G01.501.001.000")?.Id ?? Guid.Empty },
                    new User { FullName = "Trưởng Phòng Phòng 1",     RoleId = allRoles.GetValueOrDefault("TRUONG_PHONG"),      DepartmentId = allDepts.GetValueOrDefault("G01.501.001.000")?.Id ?? Guid.Empty },
                    new User { FullName = "Phó Trưởng Phòng Phòng 1", RoleId = allRoles.GetValueOrDefault("PHO_TRUONG_PHONG"), DepartmentId = allDepts.GetValueOrDefault("G01.501.001.000")?.Id ?? Guid.Empty }
                }.Where(u => u.RoleId != Guid.Empty && u.DepartmentId != Guid.Empty).ToList();

                if (!allRoles.Any())
                    _logger.LogWarning("Roles chưa được seed vào DB — bỏ qua tạo Users");

                foreach (var user in users)
                {
                    if (_unitOfWork.Users.FindFirst(u => u.FullName == user.FullName) == null)
                    {
                        _unitOfWork.Users.Add(user);
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("SetBaseData completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetBaseData: {Message}", ex.Message);
                throw;
            }
        }

        public async Task RemoveDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting RemoveData...");

                // Delete all data from all tables
                // Order matters due to foreign key constraints
                
                // EvaluationBoard - Comment related
                var comments = _unitOfWork.Comments.GetAll();
                foreach (var comment in comments)
                    _unitOfWork.Comments.Remove(comment);

                var commentFiles = _unitOfWork.CommentFiles.GetAll();
                foreach (var file in commentFiles)
                    _unitOfWork.CommentFiles.Remove(file);

                var commentAudios = _unitOfWork.CommentAudios.GetAll();
                foreach (var audio in commentAudios)
                    _unitOfWork.CommentAudios.Remove(audio);

                var commentReactions = _unitOfWork.CommentReactions.GetAll();
                foreach (var reaction in commentReactions)
                    _unitOfWork.CommentReactions.Remove(reaction);

                // EvaluationBoard
                var evaluationScores = _unitOfWork.EvaluationScores.GetAll();
                foreach (var score in evaluationScores)
                    _unitOfWork.EvaluationScores.Remove(score);

                var evaluations = _unitOfWork.Evaluations.GetAll();
                foreach (var eval in evaluations)
                    _unitOfWork.Evaluations.Remove(eval);

                // Configuration - EvaluationFlow
                var evaluationFlowCriterias = _unitOfWork.EvaluationFlowCriterias.GetAll();
                foreach (var efc in evaluationFlowCriterias)
                    _unitOfWork.EvaluationFlowCriterias.Remove(efc);

                var evaluationFlowObjects = _unitOfWork.EvaluationFlowObjects.GetAll();
                foreach (var efo in evaluationFlowObjects)
                    _unitOfWork.EvaluationFlowObjects.Remove(efo);

                var evaluationFlowRoles = _unitOfWork.EvaluationFlowRoles.GetAll();
                foreach (var efr in evaluationFlowRoles)
                    _unitOfWork.EvaluationFlowRoles.Remove(efr);

                var evaluationFlowDepartments = _unitOfWork.EvaluationFlowDepartments.GetAll();
                foreach (var efd in evaluationFlowDepartments)
                    _unitOfWork.EvaluationFlowDepartments.Remove(efd);

                var evaluationFlows = _unitOfWork.EvaluationFlows.GetAll();
                foreach (var flow in evaluationFlows)
                    _unitOfWork.EvaluationFlows.Remove(flow);

                // Configuration - EvaluationCriteria
                var classifications = _unitOfWork.Classifications.GetAll();
                foreach (var classification in classifications)
                    _unitOfWork.Classifications.Remove(classification);

                var criterias = _unitOfWork.Criterias.GetAll();
                foreach (var criteria in criterias)
                    _unitOfWork.Criterias.Remove(criteria);

                var criteriaSets = _unitOfWork.CriteriaSets.GetAll();
                foreach (var criteriaSet in criteriaSets)
                    _unitOfWork.CriteriaSets.Remove(criteriaSet);

                // Configuration - ReportType
                var reportTypeCriterias = _unitOfWork.ReportTypeCriterias.GetAll();
                foreach (var rtc in reportTypeCriterias)
                    _unitOfWork.ReportTypeCriterias.Remove(rtc);

                var reportTypes = _unitOfWork.ReportTypes.GetAll();
                foreach (var rt in reportTypes)
                    _unitOfWork.ReportTypes.Remove(rt);

                // Configuration - EvaluationObject
                var evaluationObjectRoles = _unitOfWork.EvaluationObjectRoles.GetAll();
                foreach (var eor in evaluationObjectRoles)
                    _unitOfWork.EvaluationObjectRoles.Remove(eor);

                var evaluationObjects = _unitOfWork.EvaluationObjects.GetAll();
                foreach (var eo in evaluationObjects)
                    _unitOfWork.EvaluationObjects.Remove(eo);

                // Users
                var users = _unitOfWork.Users.GetAll();
                foreach (var user in users)
                    _unitOfWork.Users.Remove(user);

                var roles = _unitOfWork.Roles.GetAll();
                foreach (var role in roles)
                    _unitOfWork.Roles.Remove(role);

                var departments = _unitOfWork.Departments.GetAll();
                foreach (var dept in departments)
                    _unitOfWork.Departments.Remove(dept);

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("RemoveData completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveData: {Message}", ex.Message);
                throw;
            }
        }
    }
}
