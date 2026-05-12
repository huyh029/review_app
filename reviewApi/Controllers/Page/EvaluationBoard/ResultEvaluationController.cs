using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Service.Page.EvaluationBoard.ResultEvaluation;

namespace reviewApi.Controllers.Page.EvaluationBoard
{
    [ApiController]
    [Route("api/page/evaluation-board/result")]
    [Authorize]
    public class ResultEvaluationController : ControllerBase
    {
        private readonly IResultEvaluationService _service;

        public ResultEvaluationController(IResultEvaluationService service)
        {
            _service = service;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] EvaluationFilterRequest filter)
        {
            var result = await _service.GetAllAsync(GetUserId(), filter);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(Guid id)
        {
            var result = await _service.GetDetailAsync(id);
            return Ok(result);
        }
    }
}
