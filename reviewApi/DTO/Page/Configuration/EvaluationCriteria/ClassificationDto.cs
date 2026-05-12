namespace reviewApi.DTO.Page.Configuration
{
    public class ClassificationDto
    {
        public Guid CriteriaSetId { get; set; }
        public string Code { get; set; }
        public string VirtualId { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public decimal? MinScore { get; set; }
        public decimal? MaxScore { get; set; }
        public int IsActive { get; set; }
    }
}
