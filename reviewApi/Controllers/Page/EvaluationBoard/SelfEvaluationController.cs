using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Service.Page.EvaluationBoard.Detail;
using reviewApi.Service.Page.EvaluationBoard.SelfEvaluation;

namespace reviewApi.Controllers.Page.EvaluationBoard
{
    [ApiController]
    [Route("api/page/evaluation-board/self")]
    [Authorize]
    public class SelfEvaluationController : ControllerBase
    {
        private readonly ISelfEvaluationService _service;
        private readonly IEvaluationCommentService _commentService;

        public SelfEvaluationController(ISelfEvaluationService service, IEvaluationCommentService commentService)
        {
            _service = service;
            _commentService = commentService;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        [HttpGet("template")]
        public async Task<IActionResult> GetNewTemplate([FromQuery] int? month, [FromQuery] int? year, [FromQuery] Guid? criteriaId)
        {
            try
            {
                var result = await _service.GetNewTemplateAsync(GetUserId(), month, year, criteriaId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEvaluationRequest request)
        {
            var result = await _service.CreateAsync(GetUserId(), request);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEvaluationRequest request)
        {
            var result = await _service.UpdateAsync(id, GetUserId(), request);
            return Ok(result);
        }

        [HttpPost("save-and-submit")]
        public async Task<IActionResult> SaveAndSubmit([FromBody] SaveAndSubmitRequest request)
        {
            try
            {
                var result = await _service.SaveAndSubmitAsync(GetUserId(), request, request.ManagerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitEvaluationRequest? request)
        {
            await _service.SubmitAsync(id, GetUserId(), request?.ManagerId);
            return Ok(new { message = "Đã nộp phiếu đánh giá" });
        }

        [HttpPost("{id}/recall")]
        public async Task<IActionResult> Recall(Guid id)
        {
            try
            {
                await _service.RecallAsync(id, GetUserId());
                return Ok(new { message = "Đã thu hồi phiếu đánh giá" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id, GetUserId());
            return Ok(new { message = "Đã xóa phiếu đánh giá" });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMany([FromBody] DeleteManyRequest request)
        {
            await _service.DeleteManyAsync(request, GetUserId());
            return Ok(new { message = "Đã xóa phiếu đánh giá" });
        }

        // Comments
        [HttpGet("detail/{evaluationId}/comments")]
        public async Task<IActionResult> GetComments(Guid evaluationId)
        {
            try
            {
                var result = await _commentService.GetCommentsAsync(evaluationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("detail/{evaluationId}/comments")]
        public async Task<IActionResult> AddComment(Guid evaluationId, [FromBody] AddCommentRequest request)
        {
            try
            {
                request.EvaluationId = evaluationId;
                var result = await _commentService.AddCommentAsync(GetUserId(), request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("detail/{evaluationId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid evaluationId, Guid commentId)
        {
            try
            {
                await _commentService.DeleteCommentAsync(commentId, GetUserId());
                return Ok(new { message = "Đã xóa bình luận" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("detail/{evaluationId}/comments/{commentId}/reactions")]
        public async Task<IActionResult> AddReaction(Guid evaluationId, Guid commentId, [FromBody] AddReactionRequest request)
        {
            try
            {
                await _commentService.AddReactionAsync(commentId, request);
                return Ok(new { message = "Đã cập nhật reaction" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
