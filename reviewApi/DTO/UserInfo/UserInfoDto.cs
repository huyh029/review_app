namespace reviewApi.DTO.UserInfo
{
    public class UserInfoDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}
