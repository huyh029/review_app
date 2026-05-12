using reviewApi.DTO.Page.EvaluationBoard;

namespace reviewApi.Service.Page.EvaluationBoard.ManagerEvaluation
{
    public interface IManagerEvaluationService
    {
        Task<EvaluationPaginatedResponse> GetAllAsync(Guid managerId, EvaluationFilterRequest filter);
        Task<EvaluationDetailDto> GetDetailAsync(Guid id);
        Task ReviewAsync(Guid id, Guid managerId);
        Task<EvaluationDetailDto> ApproveAsync(Guid id, Guid managerId, UpdateManagerScoresRequest request);
        Task<EvaluationDetailDto> UpdateScoresAsync(Guid id, Guid managerId, UpdateManagerScoresRequest request);
        Task CompleteAsync(Guid id, Guid managerId);
        List<object> GetManagers(Guid currentUserId, Guid criteriaSetId);
    }
}
