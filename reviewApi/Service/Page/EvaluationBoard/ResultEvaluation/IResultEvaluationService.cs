using reviewApi.DTO.Page.EvaluationBoard;

namespace reviewApi.Service.Page.EvaluationBoard.ResultEvaluation
{
    public interface IResultEvaluationService
    {
        Task<EvaluationPaginatedResponse> GetAllAsync(Guid userId, EvaluationFilterRequest filter);
        Task<EvaluationDetailDto> GetDetailAsync(Guid id);
    }
}
