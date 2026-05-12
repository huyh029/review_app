namespace reviewApi.Models
{
    public class CriteriaSetObject
    {
        public Guid Id { get; set; }
        public Guid CriteriaSetId { get; set; }
        public Guid EvaluationObjectId { get; set; }

        // Navigation properties
        public virtual CriteriaSet CriteriaSet { get; set; }
        public virtual EvaluationObject EvaluationObject { get; set; }
    }
}
