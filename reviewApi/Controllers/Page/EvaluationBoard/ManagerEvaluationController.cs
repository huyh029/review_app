using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Service.Page.EvaluationBoard.ManagerEvaluation;

namespace reviewApi.Controllers.Page.EvaluationBoard
{
    [ApiController]
    [Route("api/page/evaluation-board/manager")]
    [Authorize]
    public class ManagerEvaluationController : ControllerBase
    {
        private readonly IManagerEvaluationService _service;

        public ManagerEvaluationController(IManagerEvaluationService service)
        {
            _service = service;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        [HttpGet("/api/page/evaluation-board/managers")]
        public IActionResult GetManagers([FromQuery] Guid criteriaSetId)
        {
            var result = _service.GetManagers(GetUserId(), criteriaSetId);
            return Ok(result);
        }

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

        [HttpPost("{id}/review")]
        public async Task<IActionResult> Review(Guid id)
        {
            await _service.ReviewAsync(id, GetUserId());
            return Ok(new { message = "Đã trả về phiếu đánh giá" });
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] UpdateManagerScoresRequest request)
        {
            var result = await _service.ApproveAsync(id, GetUserId(), request);
            return Ok(result);
        }

        [HttpPost("{id}/update-scores")]
        public async Task<IActionResult> UpdateScores(Guid id, [FromBody] UpdateManagerScoresRequest request)
        {
            var result = await _service.UpdateScoresAsync(id, GetUserId(), request);
            return Ok(result);
        }

        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            await _service.CompleteAsync(id, GetUserId());
            return Ok(new { message = "Đã hoàn thành phiếu đánh giá" });
        }
    }
}
