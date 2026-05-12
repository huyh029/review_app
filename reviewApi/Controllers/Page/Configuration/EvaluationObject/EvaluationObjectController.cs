using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.Configuration;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-object")]
    public class EvaluationObjectController : ControllerBase
    {
        private readonly IEvaluationObjectService _service;
        private readonly ILogger<EvaluationObjectController> _logger;

        public EvaluationObjectController(IEvaluationObjectService service, ILogger<EvaluationObjectController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("GetAll endpoint called with search: {Search}, page: {Page}, pageSize: {PageSize}", search, page, pageSize);
                var result = await _service.GetAllAsync(search, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            try
            {
                _logger.LogInformation("GetByCode endpoint called for code: {Code}", code);
                var result = await _service.GetByCodeAsync(code);
                if (result == null)
                    return NotFound(new { message = "Evaluation object not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCode endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEvaluationObjectRequest request)
        {
            try
            {
                _logger.LogInformation("Create endpoint called");
                var result = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetByCode), new { code = result.Code }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPut("{code}")]
        public async Task<IActionResult> Update(string code, [FromBody] UpdateEvaluationObjectRequest request)
        {
            try
            {
                _logger.LogInformation("Update endpoint called for code: {Code}", code);
                var result = await _service.UpdateAsync(code, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> Delete(string code)
        {
            try
            {
                _logger.LogInformation("Delete endpoint called for code: {Code}", code);
                await _service.DeleteAsync(code);
                return Ok(new { message = "Evaluation object deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMany([FromBody] DeleteManyEvaluationObjectRequest request)
        {
            try
            {
                if (!request.IsAll && (request.IncludeIds == null || request.IncludeIds.Count == 0))
                    return BadRequest(new { message = "No items selected" });

                await _service.DeleteManyAsync(request);
                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteMany endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}
