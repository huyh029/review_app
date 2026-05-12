namespace reviewApi.DTO.Page.Configuration
{
    public class EvaluationObjectDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int IsActive { get; set; }
    }

    public class CreateEvaluationObjectRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class UpdateEvaluationObjectRequest
    {
        public string Name { get; set; }
        public int IsActive { get; set; }
    }

    public class DeleteManyEvaluationObjectRequest
    {
        public bool IsAll { get; set; }
        public List<string>? IncludeIds { get; set; }
        public List<string>? ExcludeIds { get; set; }
        public EvaluationObjectFilter? Filter { get; set; }
    }

    public class EvaluationObjectFilter
    {
        public string? Search { get; set; }
    }
}
