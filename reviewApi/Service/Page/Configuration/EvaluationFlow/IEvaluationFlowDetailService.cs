using reviewApi.DTO.Page.Configuration;

namespace reviewApi.Service.Page.Configuration
{
    public interface IEvaluationFlowDetailService
    {
        Task<EvaluationFlowDetailDto> GetDetailAsync(string code);
        Task<EvaluationFlowDetailDto> CreateDetailAsync(CreateEvaluationFlowDetailRequest request);
        Task<EvaluationFlowDetailDto> UpdateDetailAsync(string code, UpdateEvaluationFlowDetailRequest request);
    }
}
