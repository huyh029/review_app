namespace reviewApi.Models
{
    public class Department
    {
        public Guid Id { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public Guid? ParentId { get; set; }

        // Navigation properties
        public virtual Department Parent { get; set; }
        public virtual ICollection<Department> Children { get; set; } = new List<Department>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
