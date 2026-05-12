namespace reviewApi.DTO.Auth
{
    public class LoginRequest
    {
        public Guid UserId { get; set; }
    }
}

namespace reviewApi.DTO.Auth
{
    public class KeycloakLoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class KeycloakRefreshRequest
    {
        public string RefreshToken { get; set; }
    }

    public class KeycloakCallbackRequest
    {
        public string Code { get; set; }
        public string RedirectUri { get; set; }
    }
}
