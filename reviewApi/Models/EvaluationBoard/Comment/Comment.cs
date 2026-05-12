namespace reviewApi.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid EvaluationId { get; set; }
        public Guid UserId { get; set; }
        public string? Content { get; set; }
        public Guid? ReplyToCommentId { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Evaluation Evaluation { get; set; }
        public virtual User User { get; set; }
        public virtual Comment ReplyToComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
        public virtual ICollection<CommentFile> Files { get; set; } = new List<CommentFile>();
        public virtual ICollection<CommentAudio> Audios { get; set; } = new List<CommentAudio>();
        public virtual ICollection<CommentReaction> Reactions { get; set; } = new List<CommentReaction>();
    }
}
