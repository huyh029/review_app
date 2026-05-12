namespace reviewApi.DTO.Page.Reports
{
    public class ReportRowDto
    {
        public int Stt { get; set; }
        public string CanBo { get; set; }
        public string DonVi { get; set; }
        public decimal SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }
        public string SelfClassification { get; set; }
        public string ManagerClassification { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    public class ReportFilterRequest
    {
        public string ReportTypeCode { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReportTypeOptionDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ApplicableMonths { get; set; }
        public string ApplicableYears { get; set; }
    }

    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new();
        public PaginationInfo Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
    }
}
