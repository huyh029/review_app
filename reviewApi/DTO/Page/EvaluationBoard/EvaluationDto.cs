namespace reviewApi.DTO.Page.EvaluationBoard
{
    public class EvaluationListItemDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public string EvaluationPeriod { get; set; }
        public decimal SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }
        public string Status { get; set; }
    }

    public class EvaluationDetailDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public Guid CriteriaSetId { get; set; }
        public decimal SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }
        public string Status { get; set; }
        public List<EvaluationScoreDto> Scores { get; set; } = new();
        public List<CriteriaNodeDto> CriteriaTree { get; set; } = new();
        public List<ClassificationDto> Classifications { get; set; } = new();
    }

    public class EvaluationScoreDto
    {
        public Guid Id { get; set; }
        public string VirtualCode { get; set; }
        public decimal? SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }
    }

    public class CriteriaNodeDto
    {
        public string VirtualCode { get; set; }
        public string DisplayCode { get; set; }
        public string Content { get; set; }
        public decimal? MaxScore { get; set; }
        public string ScoreType { get; set; }
        public decimal? SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }
        public List<CriteriaNodeDto> Children { get; set; } = new();
    }

    public class ClassificationDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
    }

    public class CreateEvaluationRequest
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public Guid CriteriaSetId { get; set; }
        public List<ScoreInputDto> Scores { get; set; } = new();
    }

    public class UpdateEvaluationRequest
    {
        public List<ScoreInputDto> Scores { get; set; } = new();
    }

    public class SubmitEvaluationRequest
    {
        public Guid? ManagerId { get; set; }
    }

    public class SaveAndSubmitRequest : CreateEvaluationRequest
    {
        public Guid? ManagerId { get; set; }
    }

    public class ScoreInputDto
    {
        public string VirtualCode { get; set; }
        public decimal SelfScore { get; set; }
    }

    public class ManagerScoreInputDto
    {
        public string VirtualCode { get; set; }
        public decimal ManagerScore { get; set; }
    }

    public class UpdateManagerScoresRequest
    {
        public List<ManagerScoreInputDto> Scores { get; set; } = new();
    }

    public class EvaluationFilterRequest
    {
        public string? Status { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class CommentDto
    {
        public Guid Id { get; set; }
        public Guid EvaluationId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Content { get; set; }
        public Guid? ReplyToCommentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<CommentDto> Replies { get; set; } = new();
        public List<CommentReactionDto> Reactions { get; set; } = new();
        public List<CommentFileDto> Files { get; set; } = new();
    }

    public class CommentReactionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Emoji { get; set; }
    }

    public class CommentFileDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
    }

    public class AddCommentRequest
    {
        public Guid EvaluationId { get; set; }
        public string? Content { get; set; }
        public Guid? ReplyToCommentId { get; set; }
    }

    public class AddReactionRequest
    {
        public string Emoji { get; set; }
        public Guid UserId { get; set; }
    }

    public class NewEvaluationTemplateDto
    {
        public bool IsChanged { get; set; } = true;
        public bool IsHaveCriteria { get; set; } = true;
        public Guid CriteriaSetId { get; set; }
        public string CriteriaSetName { get; set; }
        public string FullName { get; set; }
        public string Department { get; set; }
        public int CurrentMonth { get; set; }
        public int CurrentYear { get; set; }
        public string Warning { get; set; }
        public List<CriteriaNodeDto> CriteriaTree { get; set; } = new();
        public List<ClassificationDto> Classifications { get; set; } = new();
    }

    public class DeleteManyRequest
    {
        public bool IsAll { get; set; } = false;
        public List<Guid> Ids { get; set; } = new();
        public List<Guid> ExcludeIds { get; set; } = new();
        public EvaluationFilterRequest? Filter { get; set; }
    }

    public class EvaluationPaginatedResponse
    {
        public List<EvaluationListItemDto> Data { get; set; } = new();
        public PaginationMeta Pagination { get; set; }
    }

    public class PaginationMeta
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int SelectableCount { get; set; }
    }
}
