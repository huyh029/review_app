namespace reviewApi.DTO.Page.Configuration
{
    public class CreateEvaluationCriteriaDetailRequest
    {
        public int CriteriaSetId { get; set; }
        public string CriteriaVirtualCode { get; set; }
        public int Order { get; set; }
        public int Weight { get; set; }
    }
}
