namespace reviewApi.Models
{
    public class CommentFile
    {
        public Guid Id { get; set; }
        public Guid CommentId { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
        public string FilePath { get; set; }

        // Navigation properties
        public virtual Comment Comment { get; set; }
    }
}
