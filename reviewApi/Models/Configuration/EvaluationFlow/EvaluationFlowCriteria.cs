namespace reviewApi.Models
{
    public class EvaluationFlowCriteria
    {
        public Guid Id { get; set; }
        public Guid FlowId { get; set; }
        public Guid CriteriaSetId { get; set; }

        // Navigation properties
        public virtual EvaluationFlow EvaluationFlow { get; set; }
        public virtual CriteriaSet CriteriaSet { get; set; }
    }
}
