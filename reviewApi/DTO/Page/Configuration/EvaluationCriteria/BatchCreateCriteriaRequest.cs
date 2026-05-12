namespace reviewApi.DTO.Page.Configuration
{
    public class BatchCreateCriteriaRequest
    {
        public int CriteriaSetId { get; set; }
        public List<CreateCriteriaRequest> Criteria { get; set; } = new();
    }
}
