using reviewApi.DTO.Auth;

namespace reviewApi.Service.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task LogoutAsync(Guid userId);
    }
}
