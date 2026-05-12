using reviewApi.Service.Auth;
using reviewApi.DTO.Auth;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using reviewApi.Models;

namespace reviewApi.Service.Repositories.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, IMemoryCache memoryCache, ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login attempt for UserId: {UserId}", request.UserId);

                // Get user from database with department and role
                var user = _unitOfWork.Users.GetByIdInclude(request.UserId, u => u.Department, u => u.Role);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", request.UserId);
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                _logger.LogInformation("Login successful for UserId: {UserId}", request.UserId);

                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        RoleCode = user.Role?.RoleCode,
                        RoleName = user.Role?.RoleName,
                        DepartmentCode = user.Department?.DepartmentCode,
                        DepartmentName = user.Department?.DepartmentName
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoginAsync");
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                };
            }
        }

        public async Task LogoutAsync(Guid userId)
        {
            try
            {
                _logger.LogInformation("Logout for UserId: {UserId}", userId);
                // Add user to blacklist in cache
                _memoryCache.Set($"logout_{userId}", true, TimeSpan.FromHours(24));
                _logger.LogInformation("Logout successful for UserId: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogoutAsync");
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = _configuration["JwtSettings:Key"];
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim("RoleCode", user.Role?.RoleCode ?? string.Empty),
                new Claim("DepartmentCode", user.Department?.DepartmentCode ?? string.Empty),
                new Claim("jti", Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
