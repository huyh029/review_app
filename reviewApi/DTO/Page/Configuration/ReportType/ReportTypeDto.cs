namespace reviewApi.DTO.Page.Configuration
{
    public class ReportTypeDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
        public string Criteria { get; set; }
        public int IsActive { get; set; }
    }

    public class CreateReportTypeRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
        public string Criteria { get; set; }
    }

    public class UpdateReportTypeRequest
    {
        public string Name { get; set; }
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
        public string Criteria { get; set; }
        public int IsActive { get; set; }
    }

    public class DeleteManyReportTypeRequest
    {
        public bool IsAll { get; set; }
        public List<string>? IncludeIds { get; set; }
        public List<string>? ExcludeIds { get; set; }
        public ReportTypeFilter? Filter { get; set; }
    }

    public class ReportTypeFilter
    {
        public string? Search { get; set; }
    }
}
