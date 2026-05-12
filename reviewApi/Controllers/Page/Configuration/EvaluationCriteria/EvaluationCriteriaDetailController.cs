using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.Configuration;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-criteria-detail")]
    public class EvaluationCriteriaDetailController : ControllerBase
    {
        private readonly IEvaluationCriteriaDetailService _service;
        private readonly ILogger<EvaluationCriteriaDetailController> _logger;

        public EvaluationCriteriaDetailController(IEvaluationCriteriaDetailService service, ILogger<EvaluationCriteriaDetailController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("active-evaluation-objects")]
        public async Task<IActionResult> GetActiveEvaluationObjects()
        {
            try
            {
                var result = await _service.GetActiveEvaluationObjectsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            try
            {
                var result = await _service.GetDetailAsync(id);
                if (result == null)
                    return NotFound(new { message = "Criteria set not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/detail")]
        public async Task<IActionResult> UpdateDetail(Guid id, [FromBody] UpdateCriteriaSetDetailRequest request)
        {
            try
            {
                if (request == null) return BadRequest(new { message = "Request body is required" });
                var result = await _service.UpdateDetailAsync(id, request);
                if (result == null)
                    return NotFound(new { message = "Criteria set not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
