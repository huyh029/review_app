namespace reviewApi.Models
{
    public class Classification
    {
        public Guid Id { get; set; }
        public Guid CriteriaSetId { get; set; }
        public string Code { get; set; }
        public string VirtualId { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; } // Tên viết tắt
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public int IsActive { get; set; }

        // Navigation properties
        public virtual CriteriaSet CriteriaSet { get; set; }
    }
}
