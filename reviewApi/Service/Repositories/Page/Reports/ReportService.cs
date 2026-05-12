using reviewApi.DTO.Page.Reports;
using reviewApi.Service.Page.Reports;

namespace reviewApi.Service.Repositories.Page.Reports
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PaginatedResponse<ReportRowDto>> GetReportAsync(ReportFilterRequest filter)
        {
            try
            {
                _logger.LogInformation("Getting report with filter: ReportType={ReportType}, Month={Month}, Year={Year}",
                    filter.ReportTypeCode, filter.Month, filter.Year);

                // Lấy CriteriaSetIds từ ReportType nếu có filter
                List<Guid> criteriaSetIds = null;
                if (!string.IsNullOrWhiteSpace(filter.ReportTypeCode))
                {
                    criteriaSetIds = _unitOfWork.ReportTypeCriterias.GetAll()
                        .Where(r => r.ReportType.Code == filter.ReportTypeCode)
                        .Select(r => r.CriteriaSetId)
                        .ToList();
                }

                var query = _unitOfWork.Evaluations.GetAll()
                    .Where(e => e.Status == "completed");

                if (criteriaSetIds != null && criteriaSetIds.Any())
                    query = query.Where(e => criteriaSetIds.Contains(e.CriteriaSetId));

                if (filter.Month.HasValue)
                    query = query.Where(e => e.Month == filter.Month.Value);

                if (filter.Year.HasValue)
                    query = query.Where(e => e.Year == filter.Year.Value);

                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / filter.PageSize);
                var skip = (filter.Page - 1) * filter.PageSize;

                var evaluations = query.Skip(skip).Take(filter.PageSize).ToList();

                var userIds = evaluations.Select(e => e.UserId).Distinct().ToList();
                var users = _unitOfWork.Users.GetAll()
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionary(u => u.Id);

                var deptIds = users.Values.Select(u => u.DepartmentId).Distinct().ToList();
                var departments = _unitOfWork.Departments.GetAll()
                    .Where(d => deptIds.Contains(d.Id))
                    .ToDictionary(d => d.Id);

                var mappedEvaluations = evaluations.Select((e, i) => new
                {
                    e.Id,
                    e.Month,
                    e.Year,
                    e.SelfScore,
                    e.ManagerScore,
                    UserName = users.TryGetValue(e.UserId, out var u) ? u.FullName : "",
                    DepartmentName = users.TryGetValue(e.UserId, out var u2) && departments.TryGetValue(u2.DepartmentId ?? Guid.Empty, out var d) ? d.DepartmentName : ""
                }).ToList();

                // Lấy classifications để phân loại
                var classifications = _unitOfWork.Classifications.GetAll().ToList();

                string Classify(decimal? score) =>
                    score == null ? "" :
                    classifications
                        .Where(c => c.MinScore.HasValue && c.MaxScore.HasValue
                                 && score >= c.MinScore.Value && score <= c.MaxScore.Value)
                        .Select(c => c.Name)
                        .FirstOrDefault() ?? "";

                var rows = mappedEvaluations.Select((e, i) => new ReportRowDto
                {
                    Stt = skip + i + 1,
                    CanBo = e.UserName,
                    DonVi = e.DepartmentName,
                    SelfScore = e.SelfScore,
                    ManagerScore = e.ManagerScore,
                    SelfClassification = Classify(e.SelfScore),
                    ManagerClassification = Classify(e.ManagerScore),
                    Month = e.Month,
                    Year = e.Year
                }).ToList();

                return new PaginatedResponse<ReportRowDto>
                {
                    Data = rows,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = filter.Page,
                        TotalPages = totalPages,
                        TotalItems = totalItems,
                        ItemsPerPage = filter.PageSize
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetReportAsync");
                throw;
            }
        }

        public async Task<List<ReportTypeOptionDto>> GetReportTypeOptionsAsync()
        {
            try
            {
                return _unitOfWork.ReportTypes.GetAll()
                    .Where(r => r.IsActive == 1)
                    .Select(r => new ReportTypeOptionDto
                    {
                        Code = r.Code,
                        Name = r.Name,
                        ApplicableMonths = r.ApplicableMonths,
                        ApplicableYears = r.ApplicableYears
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetReportTypeOptionsAsync");
                throw;
            }
        }
    }
}

