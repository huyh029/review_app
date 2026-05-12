using reviewApi.DTO.Page.Reports;

namespace reviewApi.Service.Page.Reports
{
    public interface IReportService
    {
        Task<PaginatedResponse<ReportRowDto>> GetReportAsync(ReportFilterRequest filter);
        Task<List<ReportTypeOptionDto>> GetReportTypeOptionsAsync();
    }
}
