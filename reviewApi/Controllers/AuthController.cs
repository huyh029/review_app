using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.Service.Auth;
using reviewApi.DTO.Auth;
using System.Net.Http.Headers;
using System.Text.Json;
using reviewApi.Service;

namespace reviewApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IAuthService authService, IConfiguration configuration, ILogger<AuthController> logger, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        // Login nội bộ (theo UserId trong DB)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                if (!response.Success)
                    return Unauthorized(response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login endpoint");
                return BadRequest(new { message = "An error occurred during login", error = ex.Message });
            }
        }

        // Login qua Keycloak (username + password)
        [HttpPost("keycloak-login")]
        public async Task<IActionResult> KeycloakLogin([FromBody] KeycloakLoginRequest request)
        {
            try
            {
                var tokenEndpoint = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/token";
                var clientId = _configuration["Keycloak:ClientId"];
                var clientSecret = _configuration["Keycloak:ClientSecret"];

                using var http = new HttpClient();
                var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret,
                    ["username"] = request.Username,
                    ["password"] = request.Password,
                    ["scope"] = "openid profile email"
                });

                var resp = await http.PostAsync(tokenEndpoint, form);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Keycloak login failed for {Username}: {Body}", request.Username, body);
                    return Unauthorized(new { message = "Sai tên đăng nhập hoặc mật khẩu" });
                }

                var tokenData = JsonSerializer.Deserialize<JsonElement>(body);
                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    access_token = tokenData.GetProperty("access_token").GetString(),
                    refresh_token = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                    expires_in = tokenData.GetProperty("expires_in").GetInt32(),
                    token_type = tokenData.GetProperty("token_type").GetString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in KeycloakLogin endpoint");
                return BadRequest(new { message = "An error occurred during login", error = ex.Message });
            }
        }

        // Keycloak Authorization Code Flow — đổi code lấy token
        [HttpPost("keycloak-callback")]
        public async Task<IActionResult> KeycloakCallback([FromBody] KeycloakCallbackRequest request)
        {
            try
            {
                var tokenEndpoint = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/token";
                using var http = new HttpClient();
                var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"]    = "authorization_code",
                    ["client_id"]     = _configuration["Keycloak:ClientId"],
                    ["client_secret"] = _configuration["Keycloak:ClientSecret"],
                    ["code"]          = request.Code,
                    ["redirect_uri"]  = request.RedirectUri,
                });
                var resp = await http.PostAsync(tokenEndpoint, form);
                var body = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Keycloak callback failed: {Body}", body);
                    return Unauthorized(new { message = "Xác thực thất bại" });
                }
                var tokenData = JsonSerializer.Deserialize<JsonElement>(body);
                return Ok(new
                {
                    access_token  = tokenData.GetProperty("access_token").GetString(),
                    refresh_token = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                    id_token      = tokenData.TryGetProperty("id_token", out var idt) ? idt.GetString() : null,
                    expires_in    = tokenData.GetProperty("expires_in").GetInt32(),
                    token_type    = tokenData.GetProperty("token_type").GetString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in KeycloakCallback");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Refresh token Keycloak
        [HttpPost("keycloak-refresh")]
        public async Task<IActionResult> KeycloakRefresh([FromBody] KeycloakRefreshRequest request)
        {
            try
            {
                var tokenEndpoint = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/token";

                using var http = new HttpClient();
                var form = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "refresh_token",
                    ["client_id"] = _configuration["Keycloak:ClientId"],
                    ["client_secret"] = _configuration["Keycloak:ClientSecret"],
                    ["refresh_token"] = request.RefreshToken
                });

                var resp = await http.PostAsync(tokenEndpoint, form);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã hết hạn" });

                var tokenData = JsonSerializer.Deserialize<JsonElement>(body);
                return Ok(new
                {
                    success = true,
                    access_token = tokenData.GetProperty("access_token").GetString(),
                    refresh_token = tokenData.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                    expires_in = tokenData.GetProperty("expires_in").GetInt32()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in KeycloakRefresh");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            try
            {
                var sub          = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var fullName     = User.FindFirst("name")?.Value
                                ?? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
                var organizeCode = User.FindFirst("organize_code")?.Value;
                var roleCode     = User.FindFirst("user_role_code")?.Value;
                bool isKeycloak  = !string.IsNullOrEmpty(organizeCode) || !string.IsNullOrEmpty(roleCode);

                reviewApi.Models.User dbUser = null;

                // 1. LocalJwt — sub chính là DB Id (chỉ áp dụng khi không phải Keycloak token)
                if (!isKeycloak && Guid.TryParse(sub, out var userId))
                    dbUser = _unitOfWork.Users.FindFirst(u => u.Id == userId);

                // 2. Keycloak — tìm theo DepartmentCode + RoleCode
                if (dbUser == null && !string.IsNullOrEmpty(organizeCode) && !string.IsNullOrEmpty(roleCode))
                {
                    var dept = _unitOfWork.Departments.FindFirst(d => d.DepartmentCode == organizeCode);
                    var role = _unitOfWork.Roles.FindFirst(r => r.RoleCode == roleCode);
                    if (dept != null && role != null)
                        dbUser = _unitOfWork.Users.FindFirst(u => u.DepartmentId == dept.Id && u.RoleId == role.Id);
                }

                // 3. Fallback: tìm theo FullName
                if (dbUser == null && !string.IsNullOrEmpty(fullName))
                    dbUser = _unitOfWork.Users.FindFirst(u => u.FullName == fullName);

                if (dbUser == null)
                    return NotFound(new { message = "Không tìm thấy user trong hệ thống" });

                var dbRole = _unitOfWork.Roles.FindFirst(r => r.Id == dbUser.RoleId);
                var dbDept = _unitOfWork.Departments.FindFirst(d => d.Id == dbUser.DepartmentId);

                return Ok(new
                {
                    id             = dbUser.Id,
                    fullName       = dbUser.FullName,
                    roleCode       = dbRole?.RoleCode ?? "",
                    roleName       = dbRole?.RoleName ?? "",
                    departmentCode = dbDept?.DepartmentCode ?? "",
                    departmentName = dbDept?.DepartmentName ?? "",
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Me endpoint");
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
                    return BadRequest(new { message = "Invalid user" });

                await _authService.LogoutAsync(userId);
                return Ok(new { message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Logout endpoint");
                return BadRequest(new { message = "An error occurred during logout", error = ex.Message });
            }
        }
    }
}
