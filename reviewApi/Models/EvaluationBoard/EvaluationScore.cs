namespace reviewApi.Models
{
    public class EvaluationScore
    {
        public Guid Id { get; set; }
        public Guid EvaluationId { get; set; }
        public string VirtualCode { get; set; }
        public decimal? SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }

        // Navigation properties
        public virtual Evaluation Evaluation { get; set; }
    }
}
