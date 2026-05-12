using reviewApi.DTO.Page.Configuration;
using reviewApi.Models;
using reviewApi.Service.Page.Configuration;

namespace reviewApi.Service.Repositories.Page.Configuration
{
    public class ReportTypeService : IReportTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportTypeService> _logger;

        public ReportTypeService(IUnitOfWork unitOfWork, ILogger<ReportTypeService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PaginatedResponse<ReportTypeDto>> GetAllAsync(string search = null, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("Getting all report types with search: {Search}, page: {Page}, pageSize: {PageSize}", search, page, pageSize);
                
                var query = _unitOfWork.ReportTypes.GetAll().AsQueryable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(r => r.Code.ToLower().Contains(search) || r.Name.ToLower().Contains(search)).AsQueryable();
                }

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                // Apply pagination
                var skip = (page - 1) * pageSize;
                var dtos = query.Skip(skip).Take(pageSize).Select(r => new ReportTypeDto
                {
                    Code = r.Code,
                    Name = r.Name,
                    ApplicableYears = r.ApplicableYears,
                    ApplicableMonths = r.ApplicableMonths,
                    Criteria = r.Criteria,
                    IsActive = r.IsActive
                }).ToList();

                return new PaginatedResponse<ReportTypeDto>
                {
                    Data = dtos,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        ItemsPerPage = pageSize
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllAsync");
                throw;
            }
        }

        public async Task<ReportTypeDto> GetByCodeAsync(string code)
        {
            try
            {
                _logger.LogInformation("Getting report type by code: {Code}", code);
                var reportType = _unitOfWork.ReportTypes.FindFirst(r => r.Code == code);
                if (reportType == null)
                {
                    _logger.LogWarning("Report type not found: {Code}", code);
                    return null;
                }

                return new ReportTypeDto
                {
                    Code = reportType.Code,
                    Name = reportType.Name,
                    ApplicableYears = reportType.ApplicableYears,
                    ApplicableMonths = reportType.ApplicableMonths,
                    Criteria = reportType.Criteria,
                    IsActive = reportType.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCodeAsync");
                throw;
            }
        }

        public async Task<ReportTypeDto> CreateAsync(CreateReportTypeRequest request)
        {
            try
            {
                _logger.LogInformation("Creating report type: {Code}", request.Code);

                // Check if already exists
                var existing = _unitOfWork.ReportTypes.FindFirst(r => r.Code == request.Code);
                if (existing != null)
                {
                    throw new Exception($"Report type with code {request.Code} already exists");
                }

                var reportType = new ReportType
                {
                    Code = request.Code,
                    Name = request.Name,
                    ApplicableYears = request.ApplicableYears,
                    ApplicableMonths = request.ApplicableMonths,
                    Criteria = request.Criteria,
                    IsActive = 1
                };

                _unitOfWork.ReportTypes.Add(reportType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Report type created successfully: {Code}", request.Code);

                return new ReportTypeDto
                {
                    Code = reportType.Code,
                    Name = reportType.Name,
                    ApplicableYears = reportType.ApplicableYears,
                    ApplicableMonths = reportType.ApplicableMonths,
                    Criteria = reportType.Criteria,
                    IsActive = reportType.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAsync");
                throw;
            }
        }

        public async Task<ReportTypeDto> UpdateAsync(string code, UpdateReportTypeRequest request)
        {
            try
            {
                _logger.LogInformation("Updating report type: {Code}", code);

                var reportType = _unitOfWork.ReportTypes.FindFirst(r => r.Code == code);
                if (reportType == null)
                {
                    throw new Exception($"Report type with code {code} not found");
                }

                reportType.Name = request.Name;
                reportType.ApplicableYears = request.ApplicableYears;
                reportType.ApplicableMonths = request.ApplicableMonths;
                reportType.Criteria = request.Criteria;
                reportType.IsActive = request.IsActive;

                _unitOfWork.ReportTypes.Update(reportType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Report type updated successfully: {Code}", code);

                return new ReportTypeDto
                {
                    Code = reportType.Code,
                    Name = reportType.Name,
                    ApplicableYears = reportType.ApplicableYears,
                    ApplicableMonths = reportType.ApplicableMonths,
                    Criteria = reportType.Criteria,
                    IsActive = reportType.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAsync");
                throw;
            }
        }

        public async Task DeleteAsync(string code)
        {
            try
            {
                _logger.LogInformation("Deleting report type: {Code}", code);

                var reportType = _unitOfWork.ReportTypes.FindFirst(r => r.Code == code);
                if (reportType == null)
                    throw new Exception($"Report type with code {code} not found");

                DeleteRelatedRecords(new List<string> { code });

                _unitOfWork.ReportTypes.Remove(reportType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Report type deleted successfully: {Code}", code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAsync");
                throw;
            }
        }

        public async Task DeleteManyAsync(DeleteManyReportTypeRequest request)
        {
            try
            {
                var query = _unitOfWork.ReportTypes.GetAll().AsQueryable();

                if (request.IsAll)
                {
                    if (!string.IsNullOrWhiteSpace(request.Filter?.Search))
                    {
                        var s = request.Filter.Search.ToLower();
                        query = query.Where(r => r.Code.ToLower().Contains(s) || r.Name.ToLower().Contains(s));
                    }
                    if (request.ExcludeIds?.Count > 0)
                        query = query.Where(r => !request.ExcludeIds.Contains(r.Code));
                }
                else
                {
                    query = query.Where(r => request.IncludeIds.Contains(r.Code));
                }

                var codes = query.Select(r => r.Code).ToList();
                _logger.LogInformation("DeleteMany: deleting {Count} report types", codes.Count);

                DeleteRelatedRecords(codes);

                var reportTypes = _unitOfWork.ReportTypes.GetAll()
                    .Where(r => codes.Contains(r.Code)).ToList();
                foreach (var r in reportTypes)
                    _unitOfWork.ReportTypes.Remove(r);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteManyAsync");
                throw;
            }
        }

        private void DeleteRelatedRecords(List<string> codes)
        {
            // Resolve ReportType.Code → Id
            var reportTypeIds = _unitOfWork.ReportTypes.GetAll()
                .Where(r => codes.Contains(r.Code))
                .Select(r => r.Id)
                .ToList();

            var reportTypeCriterias = _unitOfWork.ReportTypeCriterias.GetAll()
                .Where(rtc => reportTypeIds.Contains(rtc.ReportTypeId))
                .ToList();
            foreach (var rtc in reportTypeCriterias)
                _unitOfWork.ReportTypeCriterias.Remove(rtc);
        }
    }
}
