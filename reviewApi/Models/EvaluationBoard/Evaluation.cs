namespace reviewApi.Models
{
    public class Evaluation
    {
        public Guid Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public Guid UserId { get; set; }
        public Guid? ManagerId { get; set; }
        public Guid CriteriaSetId { get; set; }
        public string Status { get; set; }
        public decimal SelfScore { get; set; }
        public decimal? ManagerScore { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual User Manager { get; set; }
        public virtual CriteriaSet CriteriaSet { get; set; }
        public virtual ICollection<EvaluationScore> EvaluationScores { get; set; } = new List<EvaluationScore>();
    }
}
