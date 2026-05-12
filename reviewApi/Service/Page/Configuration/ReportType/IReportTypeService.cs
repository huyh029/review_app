using reviewApi.DTO.Page.Configuration;

namespace reviewApi.Service.Page.Configuration
{
    public interface IReportTypeService
    {
        Task<PaginatedResponse<ReportTypeDto>> GetAllAsync(string search = null, int page = 1, int pageSize = 10);
        Task<ReportTypeDto> GetByCodeAsync(string code);
        Task<ReportTypeDto> CreateAsync(CreateReportTypeRequest request);
        Task<ReportTypeDto> UpdateAsync(string code, UpdateReportTypeRequest request);
        Task DeleteAsync(string code);
        Task DeleteManyAsync(DeleteManyReportTypeRequest request);
    }
}
