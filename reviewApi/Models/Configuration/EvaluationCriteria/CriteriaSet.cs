namespace reviewApi.Models
{
    public class CriteriaSet
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
        public int IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<Criteria> Criterias { get; set; } = new List<Criteria>();
        public virtual ICollection<Classification> Classifications { get; set; } = new List<Classification>();
        public virtual ICollection<CriteriaSetObject> CriteriaSetObjects { get; set; } = new List<CriteriaSetObject>();
    }
}
