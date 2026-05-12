namespace reviewApi.Models
{
    public class Criteria
    {
        public Guid Id { get; set; }
        public Guid CriteriaSetId { get; set; }
        public string VirtualCode { get; set; } // 1, 1.1, 1.2, 1.2.1, ...
        public string? DisplayCode { get; set; } // Mã hiển thị (có thể trống)
        public string Content { get; set; } // Nội dung (không được trống)
        public decimal? MaxScore { get; set; } // 0-100
        public string ScoreType { get; set; } // "Cộng" hoặc "Trừ"
        public string? VirtualParentCode { get; set; } // Parent VirtualCode (có thể trống nếu là root)
        public int IsActive { get; set; }

        // Navigation properties
        public virtual CriteriaSet CriteriaSet { get; set; }
    }
}
