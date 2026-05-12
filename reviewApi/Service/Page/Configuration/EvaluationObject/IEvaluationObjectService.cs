using reviewApi.DTO.Page.Configuration;

namespace reviewApi.Service.Page.Configuration
{
    public interface IEvaluationObjectService
    {
        Task<PaginatedResponse<EvaluationObjectDto>> GetAllAsync(string search = null, int page = 1, int pageSize = 10);
        Task<EvaluationObjectDto> GetByCodeAsync(string code);
        Task<EvaluationObjectDto> CreateAsync(CreateEvaluationObjectRequest request);
        Task<EvaluationObjectDto> UpdateAsync(string code, UpdateEvaluationObjectRequest request);
        Task DeleteAsync(string code);
        Task DeleteManyAsync(DeleteManyEvaluationObjectRequest request);
    }
}
