namespace reviewApi.Models
{
    public class ReportTypeCriteria
    {
        public Guid Id { get; set; }
        public Guid ReportTypeId { get; set; }
        public Guid CriteriaSetId { get; set; }

        // Navigation properties
        public virtual ReportType ReportType { get; set; }
        public virtual CriteriaSet CriteriaSet { get; set; }
    }
}
