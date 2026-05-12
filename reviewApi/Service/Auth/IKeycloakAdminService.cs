using reviewApi.DTO.UserInfo;

namespace reviewApi.Service.Auth
{
    public interface IKeycloakAdminService
    {
        Task<List<UserInfoDto>> GetAllUsersAsync();
        Task<List<RoleInfoDto>> GetAllRolesAsync();
        Task<List<DepartmentInfoDto>> GetAllDepartmentsAsync();
    }
}
