using reviewApi.DTO.Page.Configuration;

namespace reviewApi.Service.Page.Configuration
{
    public interface IEvaluationFlowService
    {
        Task<PaginatedResponse<EvaluationFlowDto>> GetAllAsync(string search = null, int page = 1, int pageSize = 10);
        Task<EvaluationFlowDto> GetByCodeAsync(string code);
        Task<EvaluationFlowDto> CreateAsync(CreateEvaluationFlowRequest request);
        Task<EvaluationFlowDto> UpdateAsync(string code, UpdateEvaluationFlowRequest request);
        Task DeleteAsync(string code);
        Task DeleteManyAsync(DeleteManyEvaluationFlowRequest request);
    }
}
