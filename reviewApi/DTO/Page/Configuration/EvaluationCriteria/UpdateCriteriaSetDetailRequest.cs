using reviewApi.Service.General;

namespace reviewApi.DTO.Page.Configuration
{
    public class UpdateCriteriaSetDetailRequest
    {
        public string Name { get; set; }
        public List<string> ObjectCodes { get; set; } = new();
        public string ApplicableYears { get; set; }
        public string ApplicableMonths { get; set; }
        public List<CreateTreeNodeRequest> Criteria { get; set; } = new();
        public List<CreateClassificationRequest> Classifications { get; set; } = new();
    }
}
