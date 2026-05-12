using reviewApi.DTO.UserInfo;
using reviewApi.Service.Auth;
using System.Net.Http.Headers;
using System.Text.Json;

namespace reviewApi.Service.Repositories.Auth
{
    public class KeycloakAdminService : IKeycloakAdminService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<KeycloakAdminService> _logger;

        private string BaseUrl  => _configuration["Keycloak:Authority"]!.Replace("/realms/", "/admin/realms/");
        private string TokenUrl => $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/token";
        private string ClientId => _configuration["Keycloak:ClientId"]!;
        private string Secret   => _configuration["Keycloak:ClientSecret"]!;

        public KeycloakAdminService(IConfiguration configuration, ILogger<KeycloakAdminService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        // ── Admin token ───────────────────────────────────────────
        private async Task<string> GetAdminTokenAsync()
        {
            using var http = new HttpClient();
            var form = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"]    = "client_credentials",
                ["client_id"]     = ClientId,
                ["client_secret"] = Secret,
            });
            var resp = await http.PostAsync(TokenUrl, form);
            resp.EnsureSuccessStatusCode();
            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("access_token").GetString()!;
        }

        private async Task<JsonElement> GetAsync(string url, string token)
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await http.GetAsync(url);
            resp.EnsureSuccessStatusCode();
            return JsonDocument.Parse(await resp.Content.ReadAsStringAsync()).RootElement;
        }

        // ── Raw Keycloak users (dùng nội bộ) ─────────────────────
        private async Task<List<(string KcId, string FullName, string OrganizeCode, string UserRoleCode)>> FetchRawUsersAsync()
        {
            var token = await GetAdminTokenAsync();
            var result = new List<(string, string, string, string)>();
            int first = 0, max = 100;

            while (true)
            {
                var batch = await GetAsync($"{BaseUrl}/users?first={first}&max={max}", token);
                if (batch.ValueKind != JsonValueKind.Array || batch.GetArrayLength() == 0) break;

                foreach (var u in batch.EnumerateArray())
                {
                    var attrs = u.TryGetProperty("attributes", out var a) ? a : default;
                    var fullName = $"{GetProp(u, "firstName")} {GetProp(u, "lastName")}".Trim();
                    if (string.IsNullOrEmpty(fullName)) fullName = GetProp(u, "username");
                    result.Add((
                        u.GetProperty("id").GetString()!,
                        fullName,
                        GetAttr(attrs, "OrganizeCode"),
                        GetAttr(attrs, "UserRoleCode")
                    ));
                }

                if (batch.GetArrayLength() < max) break;
                first += max;
            }

            _logger.LogInformation("Keycloak: fetched {Count} raw users", result.Count);
            return result;
        }

        // ── 1. Users — map sang UserInfoDto (cấu trúc giống Model User) ──
        public async Task<List<UserInfoDto>> GetAllUsersAsync()
        {
            var raw = await FetchRawUsersAsync();

            // Build lookup maps để resolve RoleId và DepartmentId
            var roles = await GetAllRolesAsync();
            var depts = await GetAllDepartmentsAsync();

            var roleMap = roles.ToDictionary(r => r.RoleCode, r => r.Id);
            var deptMap = depts.ToDictionary(d => d.DepartmentCode, d => d.Id);

            return raw.Select(u => new UserInfoDto
            {
                Id           = Guid.Parse(u.KcId), // dùng Keycloak sub UUID làm Id
                FullName     = u.FullName,
                RoleId       = roleMap.TryGetValue(u.UserRoleCode, out var rid) ? rid : Guid.Empty,
                DepartmentId = deptMap.TryGetValue(u.OrganizeCode, out var did) ? did : Guid.Empty,
            }).ToList();
        }

        // ── 2. Roles — map sang RoleInfoDto (cấu trúc giống Model Role) ──
        public async Task<List<RoleInfoDto>> GetAllRolesAsync()
        {
            var raw = await FetchRawUsersFullAsync();

            // Build map RoleCode → RoleName ưu tiên UserRoleName attribute từ Keycloak
            var roleNameMap = raw
                .Where(u => !string.IsNullOrEmpty(u.UserRoleCode))
                .GroupBy(u => u.UserRoleCode)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault(u => !string.IsNullOrEmpty(u.UserRoleName)).UserRoleName ?? "");

            return raw
                .Where(u => !string.IsNullOrEmpty(u.UserRoleCode))
                .Select(u => u.UserRoleCode)
                .Distinct()
                .OrderBy(r => r)
                .Select(r => new RoleInfoDto
                {
                    Id       = GuidFromString(r),
                    RoleCode = r,
                    RoleName = ResolveRoleName(r, roleNameMap.TryGetValue(r, out var n) ? n : null),
                })
                .ToList();
        }

        // ── 3. Departments — map sang DepartmentInfoDto (cấu trúc giống Model Department) ──
        public async Task<List<DepartmentInfoDto>> GetAllDepartmentsAsync()
        {
            var token = await GetAdminTokenAsync();
            var raw = await FetchRawUsersAsync();

            // Build department list từ OrganizeCode + OrganizeParent
            var deptRaw = raw
                .Where(u => !string.IsNullOrEmpty(u.OrganizeCode))
                .Select(u =>
                {
                    // Lấy lại attrs đầy đủ từ raw (cần fetch lại để có OrganizeName, OrganizeParent)
                    return u;
                })
                .GroupBy(u => u.OrganizeCode)
                .ToList();

            // Fetch lại để lấy đủ OrganizeName và OrganizeParent
            var fullRaw = await FetchRawUsersFullAsync();

            // Gán Guid cố định theo thứ tự để ParentId có thể reference
            var deptList = fullRaw
                .Where(u => !string.IsNullOrEmpty(u.OrganizeCode))
                .GroupBy(u => u.OrganizeCode)
                .Select(g => g.First())
                .OrderBy(u => u.OrganizeCode)
                .ToList();

            // Map code → Guid ổn định
            var codeToGuid = deptList.ToDictionary(u => u.OrganizeCode, u => GuidFromString(u.OrganizeCode));

            return deptList.Select(u => new DepartmentInfoDto
            {
                Id             = codeToGuid[u.OrganizeCode],
                DepartmentCode = u.OrganizeCode,
                DepartmentName = u.OrganizeName,
                ParentId       = !string.IsNullOrEmpty(u.OrganizeParent) && codeToGuid.TryGetValue(u.OrganizeParent, out var pid)
                                 ? pid : null,
            }).ToList();
        }

        // ── Raw đầy đủ (có OrganizeName, OrganizeParent, UserRoleName) ─────────
        private async Task<List<(string KcId, string FullName, string OrganizeCode, string OrganizeName, string OrganizeParent, string UserRoleCode, string UserRoleName)>> FetchRawUsersFullAsync()
        {
            var token = await GetAdminTokenAsync();
            var result = new List<(string, string, string, string, string, string, string)>();
            int first = 0, max = 100;

            while (true)
            {
                var batch = await GetAsync($"{BaseUrl}/users?first={first}&max={max}", token);
                if (batch.ValueKind != JsonValueKind.Array || batch.GetArrayLength() == 0) break;

                foreach (var u in batch.EnumerateArray())
                {
                    var attrs = u.TryGetProperty("attributes", out var a) ? a : default;
                    var fullName = $"{GetProp(u, "firstName")} {GetProp(u, "lastName")}".Trim();
                    if (string.IsNullOrEmpty(fullName)) fullName = GetProp(u, "username");
                    result.Add((
                        u.GetProperty("id").GetString()!,
                        fullName,
                        GetAttr(attrs, "OrganizeCode"),
                        GetAttr(attrs, "OrganizeName"),
                        GetAttr(attrs, "OrganizeParent"),
                        GetAttr(attrs, "UserRoleCode"),
                        GetAttr(attrs, "UserRoleName")   // optional — nếu có sẽ dùng thay dictionary
                    ));
                }

                if (batch.GetArrayLength() < max) break;
                first += max;
            }

            return result;
        }

        // ── Helpers ───────────────────────────────────────────────
        /// <summary>
        /// Ưu tiên: UserRoleName (Keycloak attribute) → dictionary → UserRoleCode
        /// </summary>
        private static string ResolveRoleName(string roleCode, string kcRoleName = null)
        {
            if (!string.IsNullOrEmpty(kcRoleName)) return kcRoleName;

            return roleCode switch
            {
                "ADMIN"                 => "Administrator",
                "BO_TRUONG"             => "Bộ trưởng",
                "CAN_BO"                => "Cán bộ",
                "CHANH_VAN_PHONG"       => "Chánh Văn phòng",
                "CHIEN_SI"              => "Chiến sĩ",
                "CUC_TRUONG"            => "Cục trưởng",
                "DOI_TRUONG"            => "Đội trưởng",
                "GIAM_DOC"              => "Giám đốc",
                "GIAM_DOC_CONG"         => "Giám Đốc CTTDT",
                "GIAM_DOC_TTTTCH"       => "Giám đốc TTTTCH",
                "HIEU_TRUONG"           => "Hiệu Trưởng",
                "KE_TOAN"               => "Kế toán",
                "NHAN_VIEN"             => "Nhân viên",
                "PHO_CHANH_VAN_PHONG"   => "Phó Chánh Văn phòng",
                "PHO_CUC_TRUONG"        => "Phó Cục trưởng",
                "PHO_DOI_TRUONG"        => "Phó Đội trưởng",
                "PHO_GIAM_DOC"          => "Phó Giám đốc",
                "PHO_GIAM_DOC_CONG"     => "Phó Giám đốc CTTDT",
                "PHO_GIAM_DOC_TTTTCH"   => "Phó Giám đốc TTTTCH",
                "PHO_HIEU_TRUONG"       => "Phó Hiệu trưởng",
                "PHO_TO_TRUONG"         => "Phó Tổ trưởng",
                "PHO_TONG_CUC_TRUONG"   => "Phó Tổng cục trưởng",
                "PHO_TRUONG_BAN"        => "Phó Trưởng ban",
                "PHO_TRUONG_CONG_AN_XA" => "Phó Trưởng Công an xã/phường",
                "PHO_TRUONG_PHONG"      => "Phó Trưởng phòng",
                "PHO_TU_LENH"           => "Phó Tư lệnh",
                "PHO_VIEN_TRUONG"       => "Phó Viện Trưởng",
                "PHO_VU_TRUONG"         => "Phó Vụ trưởng",
                "THU_KY"                => "Thư ký",
                "THU_KY_LANH_DAO_BO"    => "Thư ký lãnh đạo bộ",
                "THU_TRUONG"            => "Thứ trưởng",
                "TO_TRUONG"             => "Tổ trưởng",
                "TONG_CUC_TRUONG"       => "Tổng cục trưởng",
                "TRO_LY"                => "Trợ lý",
                "TRUONG_BAN"            => "Trưởng Ban",
                "TRUONG_CONG_AN_XA"     => "Trưởng Công an xã/phường",
                "TRUONG_PHONG"          => "Trưởng phòng",
                "TU_LENH"               => "Tư Lệnh",
                "VAN_THU"               => "Văn thư",
                "VIEN_TRUONG"           => "Viện Trưởng",
                _                       => roleCode  // fallback: dùng code
            };
        }

        /// <summary>Tạo Guid ổn định từ string (dùng MD5 hash)</summary>
        private static Guid GuidFromString(string input)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
        private static string GetAttr(JsonElement attrsEl, string key)
        {
            if (attrsEl.ValueKind != JsonValueKind.Object) return "";
            if (!attrsEl.TryGetProperty(key, out var arr)) return "";
            if (arr.ValueKind == JsonValueKind.Array && arr.GetArrayLength() > 0)
                return arr[0].GetString() ?? "";
            return "";
        }

        private static string GetProp(JsonElement el, string key)
            => el.TryGetProperty(key, out var v) ? v.GetString() ?? "" : "";
    }
}
