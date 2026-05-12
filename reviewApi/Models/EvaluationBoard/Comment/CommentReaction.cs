namespace reviewApi.Models
{
    public class CommentReaction
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public string Emoji { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Comment Comment { get; set; }
        public virtual User User { get; set; }
    }
}
