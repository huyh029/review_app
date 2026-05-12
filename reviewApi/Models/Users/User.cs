namespace reviewApi.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? DepartmentId { get; set; }

        // Navigation properties
        public virtual Role Role { get; set; }
        public virtual Department Department { get; set; }
    }
}
