namespace reviewApi.Models
{
    public class ReportType
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string ApplicableYears { get; set; } // JSON array: [2024, 2025, 2026]
        public string ApplicableMonths { get; set; } // JSON array: [1, 2, 3, ...]
        public string Criteria { get; set; }
        public int IsActive { get; set; }

        // Navigation properties
        public virtual ICollection<ReportTypeCriteria> ReportTypeCriterias { get; set; } = new List<ReportTypeCriteria>();
    }
}
