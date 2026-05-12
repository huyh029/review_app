namespace reviewApi.Service.Auth
{
    public interface IKeycloakSyncService
    {
        /// <summary>Sync toàn bộ users/departments/roles từ Keycloak vào DB</summary>
        Task SyncAllAsync();

        /// <summary>Sync hoặc upsert một user cụ thể theo username Keycloak</summary>
        Task<Models.User> SyncUserAsync(string username, string organizeCode, string organizeName,
            string organizeParent, string organizeParentName, string userRoleCode, string fullName);
    }
}
