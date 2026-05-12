using Microsoft.AspNetCore.Http;
using reviewApi.DTO.Page.EvaluationBoard;

namespace reviewApi.Service.Page.EvaluationBoard.Detail
{
    public interface IEvaluationCommentService
    {
        Task<List<CommentDto>> GetCommentsAsync(Guid evaluationId);
        Task<CommentDto> AddCommentAsync(Guid userId, AddCommentRequest request);
        Task DeleteCommentAsync(Guid id, Guid userId);
        Task AddReactionAsync(Guid commentId, AddReactionRequest request);
        Task DeleteReactionAsync(Guid commentId, Guid userId);
        Task AddFileAsync(Guid commentId, IFormFile file);
    }
}
