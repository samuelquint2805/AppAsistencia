namespace AppAsistencia.Models
{
    public class RouteRole
    {

        #region Atributtes
        public bool canView { get; set; } 
        public bool canDelete { get; set; }
        public bool canEdit { get; set; }
        public bool canCreate { get; set; }

        #endregion
        #region relationships
        // Foreign keys to Route and Role
        public Guid? routeID { get; set; }
        public RoutesAs route { get; set; } = null!;

        public Guid? roleID { get; set; }
        public Role role { get; set; } = null!;



        #endregion
    }
}
