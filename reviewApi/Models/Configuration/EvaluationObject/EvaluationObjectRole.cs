namespace reviewApi.Models
{
    public class EvaluationObjectRole
    {
        public Guid Id { get; set; }
        public Guid EvaluationObjectId { get; set; }
        public Guid UserId { get; set; }

        // Navigation properties
        public virtual EvaluationObject EvaluationObject { get; set; }
        public virtual User User { get; set; }
    }
}
