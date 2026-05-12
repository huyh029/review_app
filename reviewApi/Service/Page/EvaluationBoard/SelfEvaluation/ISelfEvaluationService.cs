using reviewApi.DTO.Page.EvaluationBoard;

namespace reviewApi.Service.Page.EvaluationBoard.SelfEvaluation
{
    public interface ISelfEvaluationService
    {
        Task<EvaluationPaginatedResponse> GetAllAsync(Guid userId, EvaluationFilterRequest filter);
        Task<EvaluationDetailDto> GetDetailAsync(Guid id);
        Task<EvaluationDetailDto> CreateAsync(Guid userId, CreateEvaluationRequest request);
        Task<EvaluationDetailDto> UpdateAsync(Guid id, Guid userId, UpdateEvaluationRequest request);
        Task SubmitAsync(Guid id, Guid userId, Guid? managerId = null);
        Task<EvaluationDetailDto> SaveAndSubmitAsync(Guid userId, CreateEvaluationRequest request, Guid? managerId = null);
        Task RecallAsync(Guid id, Guid userId);
        Task DeleteAsync(Guid id, Guid userId);
        Task DeleteManyAsync(DeleteManyRequest request, Guid userId);
        Task<NewEvaluationTemplateDto> GetNewTemplateAsync(Guid userId, int? month = null, int? year = null, Guid? criteriaId = null);
    }
}
