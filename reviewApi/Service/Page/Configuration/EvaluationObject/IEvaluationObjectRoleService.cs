using reviewApi.DTO.Page.Configuration;

namespace reviewApi.Service.Page.Configuration
{
    public interface IEvaluationObjectRoleService
    {
        Task<PaginatedResponse<EvaluationObjectRoleDto>> GetAllAsync(string evaluationObjectCode, string search = null, int page = 1, int pageSize = 10);
        Task<PaginatedResponse<EvaluationObjectRoleDto>> GetAllWithoutCodeAsync(string search = null, int page = 1, int pageSize = 10);
        Task<EvaluationObjectRoleTreeResponse> GetAllAsTreeAsync();
        Task<EvaluationObjectRoleTreeResponse> SearchAsTreeAsync(string search);
        Task<List<EvaluationObjectRoleDto>> GetActiveAsync();
        Task<EvaluationObjectRoleDto> GetByIdAsync(Guid id);
        Task<EvaluationObjectRoleDto> CreateAsync(CreateEvaluationObjectRoleRequest request);
        Task<EvaluationObjectRoleDto> UpdateAsync(Guid id, UpdateEvaluationObjectRoleRequest request);
        Task DeleteAsync(Guid id);
    }
}
