using reviewApi.Models;

namespace reviewApi.Service.Repositories.Auth
{
    // Chỉ đọc từ DB — không fallback Keycloak
    // Nếu DB thiếu dữ liệu, trả về rỗng/null bình thường
    public class UserRepository : GenericRepository<User>
    {
        public UserRepository(AppDbContext context) : base(context) { }
    }

    public class RoleRepository : GenericRepository<Role>
    {
        public RoleRepository(AppDbContext context) : base(context) { }
    }

    public class DepartmentRepository : GenericRepository<Department>
    {
        public DepartmentRepository(AppDbContext context) : base(context) { }
    }
}
