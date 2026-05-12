namespace reviewApi.DTO.Page.Configuration
{
    public class TreeNodeDto
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public List<TreeNodeDto> Children { get; set; } = new List<TreeNodeDto>();
        public List<IndividualDto> Individuals { get; set; } = new List<IndividualDto>();
    }

    public class IndividualDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string RoleName { get; set; }
        public List<string> SelectedObjectIds { get; set; } = new List<string>();
    }

    public class EvaluationObjectHeaderDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class EvaluationObjectRoleTreeResponse
    {
        public TreeNodeDto Data { get; set; }
        public List<EvaluationObjectHeaderDto> Headers { get; set; } = new List<EvaluationObjectHeaderDto>();
    }
}
