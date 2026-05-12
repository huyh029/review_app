using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.Configuration;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-flow-detail")]
    public class EvaluationFlowDetailController : ControllerBase
    {
        private readonly IEvaluationFlowDetailService _service;
        private readonly ILogger<EvaluationFlowDetailController> _logger;

        public EvaluationFlowDetailController(IEvaluationFlowDetailService service, ILogger<EvaluationFlowDetailController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{code}/detail")]
        public async Task<IActionResult> GetDetail(string code)
        {
            try
            {
                _logger.LogInformation("GetDetail endpoint called for code: {Code}", code);
                var result = await _service.GetDetailAsync(code);
                if (result == null)
                    return NotFound(new { message = "Evaluation flow not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetail endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEvaluationFlowDetailRequest request)
        {
            try
            {
                _logger.LogInformation("Create endpoint called for evaluation flow detail");
                
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                var result = await _service.CreateDetailAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost("detailRequest")]
        public async Task<IActionResult> CreateDetailRequest([FromBody] CreateEvaluationFlowDetailRequest request)
        {
            try
            {
                _logger.LogInformation("CreateDetailRequest endpoint called for evaluation flow detail");
                
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                var result = await _service.CreateDetailAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateDetailRequest endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPut("{code}/detail")]
        public async Task<IActionResult> UpdateDetail(string code, [FromBody] UpdateEvaluationFlowDetailRequest request)
        {
            try
            {
                _logger.LogInformation("UpdateDetail endpoint called for code: {Code}", code);
                
                if (request == null)
                {
                    return BadRequest(new { message = "Request body is required" });
                }

                var result = await _service.UpdateDetailAsync(code, request);
                if (result == null)
                    return NotFound(new { message = "Evaluation flow not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDetail endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}
