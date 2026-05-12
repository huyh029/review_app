namespace reviewApi.DTO.UserInfo
{
    public class DepartmentInfoDto
    {
        public Guid Id { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public Guid? ParentId { get; set; }
    }
}
