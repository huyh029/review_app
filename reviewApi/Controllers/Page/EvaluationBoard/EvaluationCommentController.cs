using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.EvaluationBoard;
using reviewApi.Service.Page.EvaluationBoard.Detail;

namespace reviewApi.Controllers.Page.EvaluationBoard
{
    [ApiController]
    [Route("api/page/evaluation-board/comments")]
    [Authorize]
    public class EvaluationCommentController : ControllerBase
    {
        private readonly IEvaluationCommentService _service;

        public EvaluationCommentController(IEvaluationCommentService service)
        {
            _service = service;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());

        [HttpGet("{evaluationId}")]
        public async Task<IActionResult> GetComments(Guid evaluationId)
        {
            var result = await _service.GetCommentsAsync(evaluationId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var result = await _service.AddCommentAsync(GetUserId(), request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(Guid id)
        {
            await _service.DeleteCommentAsync(id, GetUserId());
            return Ok(new { message = "Đã xóa bình luận" });
        }

        [HttpPost("{commentId}/reactions")]
        public async Task<IActionResult> AddReaction(Guid commentId, [FromBody] AddReactionRequest request)
        {
            await _service.AddReactionAsync(commentId, request);
            return Ok(new { message = "Đã cập nhật reaction" });
        }

        [HttpDelete("{commentId}/reactions/{userId}")]
        public async Task<IActionResult> DeleteReaction(Guid commentId, Guid userId)
        {
            await _service.DeleteReactionAsync(commentId, userId);
            return Ok(new { message = "Đã xóa reaction" });
        }

        [HttpPost("{commentId}/files")]
        public async Task<IActionResult> AddFile(Guid commentId)
        {
            var file = Request.Form.Files.GetFile("file");
            if (file == null) return BadRequest("No file provided");
            try
            {
                await _service.AddFileAsync(commentId, file);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            return Ok(new { message = "Đã tải file lên" });
        }

        [HttpGet("files/{*filePath}")]
        [AllowAnonymous]
        public IActionResult StreamFile(string filePath)
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "comments", Path.GetFileName(filePath));
            if (!System.IO.File.Exists(fullPath))
                return NotFound();

            var ext = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = ext switch
            {
                ".mp4" => "video/mp4",
                ".webm" => "video/webm",
                ".mov" => "video/quicktime",
                _ => "application/octet-stream"
            };

            Response.Headers["Cache-Control"] = "no-store";
            return PhysicalFile(fullPath, "video/mp4", enableRangeProcessing: true);
        }
    }
}
