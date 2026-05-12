namespace reviewApi.DTO.Page.Configuration
{
    public class EvaluationFlowDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DepartmentCode { get; set; }
        public int IsActive { get; set; }
    }

    public class CreateEvaluationFlowRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string DepartmentCode { get; set; }
    }

    public class UpdateEvaluationFlowRequest
    {
        public string Name { get; set; }
        public string DepartmentCode { get; set; }
        public int IsActive { get; set; }
    }

    public class DeleteManyEvaluationFlowRequest
    {
        public bool IsAll { get; set; }
        public List<string>? IncludeIds { get; set; }
        public List<string>? ExcludeIds { get; set; }
        public EvaluationFlowFilter? Filter { get; set; }
    }

    public class EvaluationFlowFilter
    {
        public string? Search { get; set; }
    }
}
