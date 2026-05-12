namespace reviewApi.DTO.Page.Configuration
{
    public class CriteriaSetDetailDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
        public int IsActive { get; set; }
        public List<CriteriaDto> Criteria { get; set; } = new();
        public List<ClassificationDto> Classifications { get; set; } = new();
        public List<string> ObjectCodes { get; set; } = new();
    }
}
