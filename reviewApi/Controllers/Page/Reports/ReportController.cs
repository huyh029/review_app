using Microsoft.AspNetCore.Mvc;
using reviewApi.DTO.Page.Reports;
using reviewApi.Service.Page.Reports;

namespace reviewApi.Controllers.Page.Reports
{
    [ApiController]
    [Route("api/page/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _service;
        private readonly ILogger<ReportController> _logger;

        public ReportController(IReportService service, ILogger<ReportController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetReport(
            [FromQuery] string reportTypeCode = null,
            [FromQuery] int? month = null,
            [FromQuery] int? year = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var filter = new ReportFilterRequest
                {
                    ReportTypeCode = reportTypeCode,
                    Month = month,
                    Year = year,
                    Page = page,
                    PageSize = pageSize
                };
                var result = await _service.GetReportAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetReport endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }

        [HttpGet("report-type-options")]
        public async Task<IActionResult> GetReportTypeOptions()
        {
            try
            {
                var result = await _service.GetReportTypeOptionsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetReportTypeOptions endpoint");
                return BadRequest(new { message = "An error occurred", error = ex.Message });
            }
        }
    }
}
