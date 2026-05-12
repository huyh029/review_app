using Microsoft.AspNetCore.Mvc;
using reviewApi.Service;

namespace reviewApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly ISetupService _setupService;
        private readonly ILogger<SetupController> _logger;

        public SetupController(ISetupService setupService, ILogger<SetupController> logger)
        {
            _setupService = setupService;
            _logger = logger;
        }

        [HttpPost("set-base-data")]
        public async Task<IActionResult> SetBaseData()
        {
            try
            {
                _logger.LogInformation("SetBaseData endpoint called");
                await _setupService.SetBaseDataAsync();
                return Ok(new { message = "Base data setup completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetBaseData endpoint");
                return BadRequest(new { message = "Error setting up base data", error = ex.Message });
            }
        }

        [HttpDelete("remove-data")]
        public async Task<IActionResult> RemoveData()
        {
            try
            {
                _logger.LogInformation("RemoveData endpoint called");
                await _setupService.RemoveDataAsync();
                return Ok(new { message = "All data removed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveData endpoint");
                return BadRequest(new { message = "Error removing data", error = ex.Message });
            }
        }
    }
}
