using Microsoft.AspNetCore.Mvc;
using reviewApi.Service;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-flow-role")]
    public class EvaluationFlowRoleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvaluationFlowRoleController> _logger;

        public EvaluationFlowRoleController(IUnitOfWork unitOfWork, ILogger<EvaluationFlowRoleController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<RoleDto>>> GetRoleList()
        {
            try
            {
                _logger.LogInformation("Getting role list for evaluation flow");
                
                var roles = _unitOfWork.Roles.GetAll().ToList();
                
                if (roles == null || roles.Count == 0)
                {
                    _logger.LogWarning("No roles found");
                    return Ok(new List<RoleDto>());
                }
                
                _logger.LogInformation("Found {Count} roles", roles.Count);
                
                var result = roles.Select(r => new RoleDto
                {
                    Code = r.RoleCode,
                    Name = r.RoleName,
                    Checked = false
                }).ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRoleList endpoint");
                return StatusCode(500, new { message = "Error retrieving roles", error = ex.Message });
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<RoleDto>>> GetActiveRoles()
        {
            try
            {
                _logger.LogInformation("Getting active roles for evaluation flow");
                
                var roles = _unitOfWork.Roles.GetAll().ToList();
                
                if (roles == null || roles.Count == 0)
                {
                    _logger.LogWarning("No active roles found");
                    return Ok(new List<RoleDto>());
                }
                
                _logger.LogInformation("Found {Count} active roles", roles.Count);
                
                var result = roles.Select(r => new RoleDto
                {
                    Code = r.RoleCode,
                    Name = r.RoleName,
                    Checked = false
                }).ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActiveRoles endpoint");
                return StatusCode(500, new { message = "Error retrieving active roles", error = ex.Message });
            }
        }
    }

    public class RoleDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }
}
