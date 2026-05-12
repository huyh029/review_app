namespace reviewApi.DTO.Page.Configuration
{
    public class EvaluationFlowDetailDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<DepartmentRequest> Departments { get; set; } = new();
        public List<RoleNodeDto> Roles { get; set; } = new();
        public List<ObjectNodeDto> Objects { get; set; } = new();
        public List<string> Criteria { get; set; } = new();
        public int IsActive { get; set; }
    }

    public class DepartmentRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
    }

    public class RoleNodeDto
    {
        public string Id { get; set; }
        public string Code { get; set; }        // Virtual code: 1, 1.1, 1.2...
        public string RoleCode { get; set; }    // Actual role code (FK)
        public string Name { get; set; }        // Display name
        public List<RoleNodeDto> Children { get; set; } = new();
    }

    public class ObjectNodeDto
    {
        public string Id { get; set; }
        public string Code { get; set; }              // Virtual code: 1, 1.1, 1.2...
        public string ObjectCode { get; set; }        // Actual object code (FK)
        public string Name { get; set; }              // Display name
        public List<ObjectNodeDto> Children { get; set; } = new();
    }

    public class CreateEvaluationFlowDetailRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public List<string> Departments { get; set; } = new();
        public List<RoleNodeDto> Roles { get; set; } = new();
        public List<ObjectNodeDto> Objects { get; set; } = new();
        public List<string> Criteria { get; set; } = new();
    }

    public class UpdateEvaluationFlowDetailRequest
    {
        public string Name { get; set; }
        public List<string> Departments { get; set; } = new();
        public List<RoleNodeDto> Roles { get; set; } = new();
        public List<ObjectNodeDto> Objects { get; set; } = new();
        public List<string> Criteria { get; set; } = new();
        public int IsActive { get; set; }
    }
}
