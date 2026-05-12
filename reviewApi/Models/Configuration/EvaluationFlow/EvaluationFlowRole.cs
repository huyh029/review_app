namespace reviewApi.Models
{
    public class EvaluationFlowRole
    {
        public Guid Id { get; set; }
        public Guid FlowId { get; set; }
        public string VirtualCode { get; set; } // 1, 1.1, 1.2, 1.2.1, ...
        public Guid RoleId { get; set; }
        public string? VirtualParentCode { get; set; } // Parent VirtualCode (có thể trống nếu là root)

        // Navigation properties
        public virtual EvaluationFlow EvaluationFlow { get; set; }
        public virtual Role Role { get; set; }
    }
}
