namespace reviewApi.DTO.Page.Configuration
{
    public class BatchCreateClassificationRequest
    {
        public int CriteriaSetId { get; set; }
        public List<CreateClassificationRequest> Classifications { get; set; } = new();
    }
}
