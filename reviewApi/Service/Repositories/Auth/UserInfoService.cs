using Microsoft.Extensions.Caching.Memory;
using reviewApi.DTO.UserInfo;
using reviewApi.Models;
using reviewApi.Service.Auth;

namespace reviewApi.Service.Repositories.Auth
{
    public class UserInfoService : IUserInfoService
    {
        private readonly AppDbContext _context;
        private readonly IKeycloakAdminService _keycloak;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserInfoService> _logger;

        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
        private const string CacheKeyUsers       = "kc_users";
        private const string CacheKeyRoles       = "kc_roles";
        private const string CacheKeyDepartments = "kc_departments";

        public UserInfoService(AppDbContext context, IKeycloakAdminService keycloak,
            IMemoryCache cache, ILogger<UserInfoService> logger)
        {
            _context  = context;
            _keycloak = keycloak;
            _cache    = cache;
            _logger   = logger;
        }

        public async Task<List<UserInfoDto>> GetAllUsersAsync()
        {
            var dbUsers = _context.Users.ToList();
            if (dbUsers.Count() > 0)
                return dbUsers.Select(u => new UserInfoDto
                {
                    Id           = u.Id,
                    FullName     = u.FullName,
                    RoleId       = u.RoleId,
                    DepartmentId = u.DepartmentId,
                }).ToList();

            if (_cache.TryGetValue(CacheKeyUsers, out List<UserInfoDto> cached))
            {
                _logger.LogInformation("UserInfo: {Count} users từ cache", cached!.Count);
                return cached!;
            }

            _logger.LogInformation("UserInfo: DB rỗng, gọi Keycloak lấy users...");
            var result = await _keycloak.GetAllUsersAsync();
            _cache.Set(CacheKeyUsers, result, CacheDuration);
            return result;
        }

        public async Task<List<RoleInfoDto>> GetAllRolesAsync()
        {
            var dbRoles = _context.Roles.ToList();
            if (dbRoles.Count() > 0)
                return dbRoles.Select(r => new RoleInfoDto
                {
                    Id       = r.Id,
                    RoleCode = r.RoleCode,
                    RoleName = r.RoleName,
                }).ToList();

            if (_cache.TryGetValue(CacheKeyRoles, out List<RoleInfoDto> cached))
            {
                _logger.LogInformation("UserInfo: {Count} roles từ cache", cached!.Count);
                return cached!;
            }

            _logger.LogInformation("UserInfo: DB rỗng, gọi Keycloak lấy roles...");
            var result = await _keycloak.GetAllRolesAsync();
            _cache.Set(CacheKeyRoles, result, CacheDuration);
            return result;
        }

        public async Task<List<DepartmentInfoDto>> GetAllDepartmentsAsync()
        {
            var dbDepts = _context.Departments.ToList();
            if (dbDepts.Count() > 0)
                return dbDepts.Select(d => new DepartmentInfoDto
                {
                    Id             = d.Id,
                    DepartmentCode = d.DepartmentCode,
                    DepartmentName = d.DepartmentName,
                    ParentId       = d.ParentId,
                }).ToList();

            if (_cache.TryGetValue(CacheKeyDepartments, out List<DepartmentInfoDto> cached))
            {
                _logger.LogInformation("UserInfo: {Count} departments từ cache", cached!.Count);
                return cached!;
            }

            _logger.LogInformation("UserInfo: DB rỗng, gọi Keycloak lấy departments...");
            var result = await _keycloak.GetAllDepartmentsAsync();
            _cache.Set(CacheKeyDepartments, result, CacheDuration);
            return result;
        }
    }
}

