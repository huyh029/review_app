namespace reviewApi.Models
{
    public class Role
    {
        public Guid Id { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
