using reviewApi.DTO.Page.Configuration;

namespace reviewApi.Service.Page.Configuration
{
    public interface IEvaluationCriteriaDetailService
    {
        Task<List<EvaluationObjectDto>> GetActiveEvaluationObjectsAsync();
        Task<CriteriaSetDetailDto> GetDetailAsync(Guid id);
        Task<CriteriaSetDetailDto> UpdateDetailAsync(Guid id, UpdateCriteriaSetDetailRequest request);
    }
}
