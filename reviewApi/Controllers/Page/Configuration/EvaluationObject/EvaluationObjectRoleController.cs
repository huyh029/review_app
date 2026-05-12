using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.Configuration;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-object/{evaluationObjectCode}/role")]
    public class EvaluationObjectRoleController : ControllerBase
    {
        private readonly IEvaluationObjectRoleService _service;
        private readonly ILogger<EvaluationObjectRoleController> _logger;

        public EvaluationObjectRoleController(IEvaluationObjectRoleService service, ILogger<EvaluationObjectRoleController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromRoute] string evaluationObjectCode, [FromQuery] string search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _service.GetAllAsync(evaluationObjectCode, search, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Evaluation object role not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromRoute] string evaluationObjectCode, [FromBody] CreateEvaluationObjectRoleRequest request)
        {
            try
            {
                request.EvaluationObjectCode = evaluationObjectCode;
                var result = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEvaluationObjectRoleRequest request)
        {
            try
            {
                var result = await _service.UpdateAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return Ok(new { message = "Evaluation object role deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}
