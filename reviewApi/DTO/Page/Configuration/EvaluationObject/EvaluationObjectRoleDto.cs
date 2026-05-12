namespace reviewApi.DTO.Page.Configuration
{
    public class EvaluationObjectRoleDto
    {
        public Guid Id { get; set; }
        public string EvaluationObjectCode { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
    }

    public class CreateEvaluationObjectRoleRequest
    {
        public string EvaluationObjectCode { get; set; }
        public Guid UserId { get; set; }
    }

    public class UpdateEvaluationObjectRoleRequest
    {
        public Guid UserId { get; set; }
    }
}
