using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.Configuration;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-criteria")]
    public class EvaluationCriteriaController : ControllerBase
    {
        private readonly IEvaluationCriteriaService _service;
        private readonly ILogger<EvaluationCriteriaController> _logger;

        public EvaluationCriteriaController(IEvaluationCriteriaService service, ILogger<EvaluationCriteriaController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetAllAsync(search, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Criteria set not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCriteriaSetRequest request)
        {
            try
            {
                var result = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCriteriaSetRequest request)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Criteria set deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMany([FromBody] DeleteManyCriteriaSetRequest request)
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("criteria")]
        public async Task<IActionResult> CreateCriteria([FromBody] CreateCriteriaRequest request)
        {
            try
            {
                if (request == null) return BadRequest(new { message = "Request body is required" });
                var result = await _service.CreateCriteriaAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("batch")]
        public async Task<IActionResult> CreateCriteriaSetDetail([FromBody] CreateCriteriaSetDetailRequest request)
        {
            try
            {
                if (request == null) return BadRequest(new { message = "Request body is required" });
                var result = await _service.CreateCriteriaSetDetailAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
