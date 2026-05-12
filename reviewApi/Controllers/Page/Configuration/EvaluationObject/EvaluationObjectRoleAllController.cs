using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.Configuration;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-object-role")]
    public class EvaluationObjectRoleAllController : ControllerBase
    {
        private readonly IEvaluationObjectRoleService _service;
        private readonly ILogger<EvaluationObjectRoleAllController> _logger;

        public EvaluationObjectRoleAllController(IEvaluationObjectRoleService service, ILogger<EvaluationObjectRoleAllController> _logger)
        {
            _service = service;
            this._logger = _logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("GetAll endpoint called for all evaluation object roles as tree");
                var result = await _service.GetAllAsTreeAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            try
            {
                _logger.LogInformation("GetActive endpoint called");
                var result = await _service.GetActiveAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActive endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            try
            {
                _logger.LogInformation("Search endpoint called with query: {Query}", q);
                
                if (string.IsNullOrWhiteSpace(q))
                {
                    return BadRequest(new { message = "Search query is required" });
                }

                var result = await _service.SearchAsTreeAsync(q);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Search endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> BatchSave([FromBody] List<CreateEvaluationObjectRoleRequest> requests)
        {
            try
            {
                _logger.LogInformation("BatchSave endpoint called with {Count} requests", requests.Count);
                
                if (requests == null || requests.Count == 0)
                {
                    return BadRequest(new { message = "No requests provided" });
                }

                // For each request, create or update the role
                foreach (var request in requests)
                {
                    await _service.CreateAsync(request);
                }

                return Ok(new { message = "Roles saved successfully", count = requests.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BatchSave endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}

