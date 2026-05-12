namespace reviewApi.DTO.Page.Configuration
{
    public class CriteriaSetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
        public int IsActive { get; set; }
        public List<string> ObjectCodes { get; set; } = new();
    }

    public class CreateCriteriaSetRequest
    {
        public string Name { get; set; }
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
    }

    public class UpdateCriteriaSetRequest
    {
        public string Name { get; set; }
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
        public int IsActive { get; set; }
    }

    public class DeleteManyCriteriaSetRequest
    {
        public bool IsAll { get; set; }
        public List<Guid>? IncludeIds { get; set; }
        public List<Guid>? ExcludeIds { get; set; }
        public CriteriaSetFilter? Filter { get; set; }
    }

    public class CriteriaSetFilter
    {
        public string? Search { get; set; }
    }
}
