namespace reviewApi.Models
{
    public class EvaluationFlow
    {
        public Guid Id { get; set; }
        public string FlowCode { get; set; }
        public string FlowName { get; set; }
        public int IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<EvaluationFlowDepartment> Departments { get; set; } = new List<EvaluationFlowDepartment>();
        public virtual ICollection<EvaluationFlowRole> Roles { get; set; } = new List<EvaluationFlowRole>();
        public virtual ICollection<EvaluationFlowObject> Objects { get; set; } = new List<EvaluationFlowObject>();
        public virtual ICollection<EvaluationFlowCriteria> Criterias { get; set; } = new List<EvaluationFlowCriteria>();
    }
}
