namespace AppAsistencia.Models
{
    public class RouteRole
    {
        #region relationships
        // Foreign keys to Route and Role
        public Guid? routeID { get; set; }
        public RoutesAs route { get; set; } = null!;

        public Guid? roleID { get; set; }
        public Role role { get; set; } = null!;

        #endregion
    }
}
