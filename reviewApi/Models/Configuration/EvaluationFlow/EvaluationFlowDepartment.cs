namespace reviewApi.Models
{
    public class EvaluationFlowDepartment
    {
        public Guid Id { get; set; }
        public Guid FlowId { get; set; }
        public Guid DepartmentId { get; set; }

        // Navigation properties
        public virtual EvaluationFlow EvaluationFlow { get; set; }
        public virtual Department Department { get; set; }
    }
}
