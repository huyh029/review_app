using Microsoft.EntityFrameworkCore;
using reviewApi.Models;
using reviewApi.Service.Auth;
using reviewApi.DTO.UserInfo;

namespace reviewApi.Service.Repositories.Auth
{
    /// <summary>
    /// Đồng bộ dữ liệu từ Keycloak vào DB.
    /// Logic upsert: so sánh từng record, chỉ insert/update khi có thay đổi.
    /// </summary>
    public class KeycloakSyncService : IKeycloakSyncService
    {
        private readonly AppDbContext _context;
        private readonly IKeycloakAdminService _keycloak;
        private readonly ILogger<KeycloakSyncService> _logger;

        public KeycloakSyncService(AppDbContext context, IKeycloakAdminService keycloak,
            ILogger<KeycloakSyncService> logger)
        {
            _context  = context;
            _keycloak = keycloak;
            _logger   = logger;
        }

        /// <summary>
        /// Sync toàn bộ Departments → Roles → Users theo thứ tự (FK dependency).
        /// </summary>
        public async Task SyncAllAsync()
        {
            _logger.LogInformation("[Sync] Bắt đầu đồng bộ Keycloak → DB...");
            try
            {
                await SyncDepartmentsAsync();
                await SyncRolesAsync();
                await SyncUsersAsync();
                _logger.LogInformation("[Sync] Hoàn thành đồng bộ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Sync] Lỗi khi đồng bộ: {Message}", ex.Message);
                throw;
            }
        }

        // ── Departments ──────────────────────────────────────────────

        private async Task SyncDepartmentsAsync()
        {
            var kcDepts = await _keycloak.GetAllDepartmentsAsync();
            var dbDepts = await _context.Departments.ToDictionaryAsync(d => d.Id);

            var kcMap = kcDepts.ToDictionary(d => d.Id);

            int added = 0, updated = 0, removed = 0;

            // Sắp xếp topo: cha trước, con sau (tránh FK violation khi insert)
            var sorted = TopologicalSort(kcDepts);

            foreach (var kc in sorted)
            {
                if (dbDepts.TryGetValue(kc.Id, out var db))
                {
                    // Đã có → kiểm tra thay đổi
                    if (db.DepartmentCode != kc.DepartmentCode ||
                        db.DepartmentName != kc.DepartmentName ||
                        db.ParentId       != kc.ParentId)
                    {
                        db.DepartmentCode = kc.DepartmentCode;
                        db.DepartmentName = kc.DepartmentName;
                        db.ParentId       = kc.ParentId;
                        updated++;
                    }
                }
                else
                {
                    // Chưa có → thêm mới
                    _context.Departments.Add(new Department
                    {
                        Id             = kc.Id,
                        DepartmentCode = kc.DepartmentCode,
                        DepartmentName = kc.DepartmentName,
                        ParentId       = kc.ParentId
                    });
                    added++;
                }
            }

            // Xóa những department không còn trong Keycloak
            foreach (var db in dbDepts.Values)
            {
                if (!kcMap.ContainsKey(db.Id))
                {
                    _context.Departments.Remove(db);
                    removed++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[Sync] Departments: +{Added} ~{Updated} -{Removed}", added, updated, removed);
        }

        // ── Roles ────────────────────────────────────────────────────

        private async Task SyncRolesAsync()
        {
            var kcRoles = await _keycloak.GetAllRolesAsync();
            var dbRoles = await _context.Roles.ToDictionaryAsync(r => r.Id);

            var kcMap = kcRoles.ToDictionary(r => r.Id);

            int added = 0, updated = 0, removed = 0;

            foreach (var kc in kcRoles)
            {
                if (dbRoles.TryGetValue(kc.Id, out var db))
                {
                    if (db.RoleCode != kc.RoleCode || db.RoleName != kc.RoleName)
                    {
                        db.RoleCode = kc.RoleCode;
                        db.RoleName = kc.RoleName;
                        updated++;
                    }
                }
                else
                {
                    _context.Roles.Add(new Role
                    {
                        Id       = kc.Id,
                        RoleCode = kc.RoleCode,
                        RoleName = kc.RoleName
                    });
                    added++;
                }
            }

            foreach (var db in dbRoles.Values)
            {
                if (!kcMap.ContainsKey(db.Id))
                {
                    _context.Roles.Remove(db);
                    removed++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[Sync] Roles: +{Added} ~{Updated} -{Removed}", added, updated, removed);
        }

        // ── Users ────────────────────────────────────────────────────

        private async Task SyncUsersAsync()
        {
            var kcUsers = await _keycloak.GetAllUsersAsync();
            var dbUsers = await _context.Users.ToDictionaryAsync(u => u.Id);

            var kcMap = kcUsers.ToDictionary(u => u.Id);

            int added = 0, updated = 0, removed = 0;

            foreach (var kc in kcUsers)
            {
                if (dbUsers.TryGetValue(kc.Id, out var db))
                {
                    if (db.FullName     != kc.FullName ||
                        db.RoleId       != kc.RoleId   ||
                        db.DepartmentId != kc.DepartmentId)
                    {
                        db.FullName     = kc.FullName;
                        db.RoleId       = kc.RoleId;
                        db.DepartmentId = kc.DepartmentId;
                        updated++;
                    }
                }
                else
                {
                    _context.Users.Add(new User
                    {
                        Id           = kc.Id,
                        FullName     = kc.FullName,
                        RoleId       = kc.RoleId,
                        DepartmentId = kc.DepartmentId
                    });
                    added++;
                }
            }

            foreach (var db in dbUsers.Values)
            {
                if (!kcMap.ContainsKey(db.Id))
                {
                    _context.Users.Remove(db);
                    removed++;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("[Sync] Users: +{Added} ~{Updated} -{Removed}", added, updated, removed);
        }

        // ── SyncUserAsync (upsert 1 user) ────────────────────────────

        public async Task<User> SyncUserAsync(string username, string organizeCode, string organizeName,
            string organizeParent, string organizeParentName, string userRoleCode, string fullName)
        {
            // Upsert Department
            var allDepts = await _keycloak.GetAllDepartmentsAsync();
            var deptInfo = allDepts.FirstOrDefault(d => d.DepartmentCode == organizeCode);
            if (deptInfo != null)
            {
                var dept = await _context.Departments.FindAsync(deptInfo.Id);
                if (dept == null)
                {
                    dept = new Department
                    {
                        Id             = deptInfo.Id,
                        DepartmentCode = deptInfo.DepartmentCode,
                        DepartmentName = deptInfo.DepartmentName,
                        ParentId       = deptInfo.ParentId
                    };
                    _context.Departments.Add(dept);
                    await _context.SaveChangesAsync();
                }
            }

            // Upsert Role
            var allRoles = await _keycloak.GetAllRolesAsync();
            var roleInfo = allRoles.FirstOrDefault(r => r.RoleCode == userRoleCode);
            if (roleInfo != null)
            {
                var role = await _context.Roles.FindAsync(roleInfo.Id);
                if (role == null)
                {
                    _context.Roles.Add(new Role
                    {
                        Id       = roleInfo.Id,
                        RoleCode = roleInfo.RoleCode,
                        RoleName = roleInfo.RoleName
                    });
                    await _context.SaveChangesAsync();
                }
            }

            // Upsert User
            var allUsers = await _keycloak.GetAllUsersAsync();
            var userInfo = allUsers.FirstOrDefault(u => u.FullName == fullName);
            if (userInfo == null) return null;

            var user = await _context.Users.FindAsync(userInfo.Id);
            if (user == null)
            {
                user = new User
                {
                    Id           = userInfo.Id,
                    FullName     = userInfo.FullName,
                    RoleId       = userInfo.RoleId,
                    DepartmentId = userInfo.DepartmentId
                };
                _context.Users.Add(user);
            }
            else
            {
                user.FullName     = userInfo.FullName;
                user.RoleId       = userInfo.RoleId;
                user.DepartmentId = userInfo.DepartmentId;
            }

            await _context.SaveChangesAsync();
            return user;
        }

        // ── Helpers ──────────────────────────────────────────────────

        private static List<DepartmentInfoDto> TopologicalSort(List<DepartmentInfoDto> depts)
        {
            var map     = depts.ToDictionary(d => d.Id);
            var sorted  = new List<DepartmentInfoDto>();
            var visited = new HashSet<Guid>();

            void Visit(DepartmentInfoDto d)
            {
                if (visited.Contains(d.Id)) return;
                visited.Add(d.Id);
                if (d.ParentId.HasValue && map.TryGetValue(d.ParentId.Value, out var parent))
                    Visit(parent);
                sorted.Add(d);
            }

            foreach (var d in depts) Visit(d);
            return sorted;
        }
    }
}
