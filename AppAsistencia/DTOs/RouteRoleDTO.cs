using AppAsistencia.Models;

namespace AppAsistencia.DTOs
{
    public class RouteRoleDTO
    {
        #region relationships
        // Foreign keys to Route and Role
        public Guid? routeID { get; set; }
        public RoutesASDTO routeDTO { get; set; } = null!;

        public Guid? roleID { get; set; }
        public RoleDTO roleDTO { get; set; } = null!;

        #endregion
    }
}
