using Microsoft.AspNetCore.Mvc;
using reviewApi.Service;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-flow-object")]
    public class EvaluationFlowObjectController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EvaluationFlowObjectController> _logger;

        public EvaluationFlowObjectController(IUnitOfWork unitOfWork, ILogger<EvaluationFlowObjectController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet("list")]
        public async Task<ActionResult<List<EvaluationObjectDto>>> GetObjectList()
        {
            try
            {
                _logger.LogInformation("Getting active evaluation object list for evaluation flow");
                
                var objects = _unitOfWork.EvaluationObjects.GetAll().Where(o => o.IsActive == 1).ToList();
                
                if (objects == null || objects.Count == 0)
                {
                    _logger.LogWarning("No active evaluation objects found");
                    return Ok(new List<EvaluationObjectDto>());
                }
                
                _logger.LogInformation("Found {Count} active evaluation objects", objects.Count);
                
                var result = objects.Select(o => new EvaluationObjectDto
                {
                    Code = o.Code,
                    Name = o.Name,
                    Checked = false
                }).ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetObjectList endpoint");
                return StatusCode(500, new { message = "Error retrieving evaluation objects", error = ex.Message });
            }
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<EvaluationObjectDto>>> GetActiveObjects()
        {
            try
            {
                _logger.LogInformation("Getting active evaluation objects for evaluation flow");
                
                var objects = _unitOfWork.EvaluationObjects.GetAll().Where(o => o.IsActive == 1).ToList();
                
                if (objects == null || objects.Count == 0)
                {
                    _logger.LogWarning("No active evaluation objects found");
                    return Ok(new List<EvaluationObjectDto>());
                }
                
                _logger.LogInformation("Found {Count} active evaluation objects", objects.Count);
                
                var result = objects.Select(o => new EvaluationObjectDto
                {
                    Code = o.Code,
                    Name = o.Name,
                    Checked = false
                }).ToList();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActiveObjects endpoint");
                return StatusCode(500, new { message = "Error retrieving active evaluation objects", error = ex.Message });
            }
        }
    }

    public class EvaluationObjectDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
    }
}
