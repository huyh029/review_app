using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.Configuration;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-flow-criteria")]
    public class EvaluationFlowCriteriaController : ControllerBase
    {
        private readonly IEvaluationCriteriaService _evaluationCriteriaService;
        private readonly ILogger<EvaluationFlowCriteriaController> _logger;

        public EvaluationFlowCriteriaController(IEvaluationCriteriaService evaluationCriteriaService, ILogger<EvaluationFlowCriteriaController> logger)
        {
            _evaluationCriteriaService = evaluationCriteriaService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<CriteriaSetDto>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all criteria sets for evaluation flow");
                
                var response = await _evaluationCriteriaService.GetAllAsync(pageSize: 1000);
                
                _logger.LogInformation("Found {Count} criteria sets", response.Data.Count);
                
                return Ok(response.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll endpoint");
                return StatusCode(500, new { message = "Error retrieving criteria sets", error = ex.Message });
            }
        }
    }
}
