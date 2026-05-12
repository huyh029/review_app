using Microsoft.AspNetCore.Mvc;
using reviewApi.Service;
using reviewApi.Service.General;

namespace reviewApi.Controllers.Page.Configuration
{
    [ApiController]
    [Route("api/page/configuration/evaluation-flow-department")]
    public class EvaluationFlowDepartmentController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITreeBuilderService _treeBuilderService;
        private readonly ILogger<EvaluationFlowDepartmentController> _logger;

        public EvaluationFlowDepartmentController(IUnitOfWork unitOfWork, ITreeBuilderService treeBuilderService, ILogger<EvaluationFlowDepartmentController> logger)
        {
            _unitOfWork = unitOfWork;
            _treeBuilderService = treeBuilderService;
            _logger = logger;
        }

        [HttpGet("tree")]
        public async Task<ActionResult<List<DepartmentTreeDto>>> GetDepartmentTree()
        {
            try
            {
                _logger.LogInformation("Getting department tree for evaluation flow");
                
                var allDepartments = _unitOfWork.Departments.GetAll().ToList();

                var idToCode = allDepartments.ToDictionary(d => d.Id, d => d.DepartmentCode);

                var tree = _treeBuilderService.BuildTree(
                    allDepartments,
                    d => d.DepartmentCode,
                    d => d.ParentId.HasValue && idToCode.TryGetValue(d.ParentId.Value, out var parentCode)
                         ? parentCode : null,
                    d => new TreeNodeDto<Models.Department> { Data = d }
                );
                
                _logger.LogInformation("Found {Count} root departments", tree.Count);
                
                var result = tree.Select(t => ConvertToDto(t)).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDepartmentTree endpoint");
                return StatusCode(500, new { message = "Error retrieving departments", error = ex.Message });
            }
        }

        [HttpGet("treeRequest")]
        public async Task<ActionResult<List<DepartmentTreeDto>>> GetDepartmentTreeRequest()
        {
            try
            {
                _logger.LogInformation("Getting department tree request for evaluation flow");
                
                var allDepartments = _unitOfWork.Departments.GetAll().ToList();
                
                if (allDepartments == null || allDepartments.Count == 0)
                {
                    _logger.LogWarning("No departments found");
                    return Ok(new List<DepartmentTreeDto>());
                }
                
                _logger.LogInformation("Total departments: {Count}", allDepartments.Count);

                // Build lookup: Id → DepartmentCode để resolve ParentId → ParentCode
                var idToCode = allDepartments.ToDictionary(d => d.Id, d => d.DepartmentCode);

                var tree = _treeBuilderService.BuildTree(
                    allDepartments,
                    d => d.DepartmentCode,
                    d => d.ParentId.HasValue && idToCode.TryGetValue(d.ParentId.Value, out var parentCode)
                         ? parentCode : null,
                    d => new TreeNodeDto<Models.Department> { Data = d }
                );
                
                _logger.LogInformation("Found {Count} root departments", tree.Count);
                
                var result = tree.Select(t => ConvertToDto(t)).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDepartmentTreeRequest endpoint");
                return StatusCode(500, new { message = "Error retrieving departments", error = ex.Message });
            }
        }

        private DepartmentTreeDto ConvertToDto(TreeNodeDto<Models.Department> node)
        {
            return new DepartmentTreeDto
            {
                Id = node.Code,
                Code = node.Code,
                Name = node.Data.DepartmentName,
                Checked = false,
                Expanded = true,
                Children = node.Children != null && node.Children.Count > 0 ? node.Children.Select(c => ConvertToDto(c)).ToList() : null
            };
        }
    }

    public class DepartmentTreeDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Checked { get; set; }
        public bool Expanded { get; set; }
        public List<DepartmentTreeDto> Children { get; set; }
    }
}
