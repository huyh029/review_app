using reviewApi.DTO.Page.Configuration;

namespace reviewApi.Service.Page.Configuration
{
    public interface IEvaluationCriteriaService
    {
        Task<PaginatedResponse<CriteriaSetDto>> GetAllAsync(string search = null, int page = 1, int pageSize = 10);
        Task<CriteriaSetDto> GetByIdAsync(Guid id);
        Task<CriteriaSetDto> CreateAsync(CreateCriteriaSetRequest request);
        Task<CriteriaSetDto> UpdateAsync(Guid id, UpdateCriteriaSetRequest request);
        Task DeleteAsync(Guid id);
        Task DeleteManyAsync(DeleteManyCriteriaSetRequest request);
        Task<CriteriaDto> CreateCriteriaAsync(CreateCriteriaRequest request);
        Task<CriteriaSetDto> CreateCriteriaSetDetailAsync(CreateCriteriaSetDetailRequest request);
        Task<CriteriaSetDetailDto> GetDetailByIdAsync(Guid id);
    }
}
