namespace reviewApi.Models
{
    public class CommentAudio
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public string AudioPath { get; set; }
        public int DurationSeconds { get; set; }
        public string AudioType { get; set; }

        // Navigation properties
        public virtual Comment Comment { get; set; }
    }
}
