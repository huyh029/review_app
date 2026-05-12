namespace reviewApi.DTO.Page.Configuration
{
    public class CriteriaDto
    {
        public Guid CriteriaSetId { get; set; }
        public string VirtualCode { get; set; }
        public string DisplayCode { get; set; }
        public string Content { get; set; }
        public decimal? MaxScore { get; set; }
        public string ScoreType { get; set; }
        public string VirtualParentCode { get; set; }
        public int IsActive { get; set; }
    }
}
