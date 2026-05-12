namespace reviewApi.Models
{
    public class EvaluationObject
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<EvaluationObjectRole> EvaluationObjectRoles { get; set; } = new List<EvaluationObjectRole>();
    }
}
