using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class RoutesAs
    {

        #region atributtes 
        [Key]
        [Required]
        public Guid? idRoute { get; set; }

        [Required]
        public string routeName { get; set; }
        #endregion

        #region relationships

        // One-to-Many relationship with RouteRole
        public List<RouteRole> routeRoles { get; } = [];
        #endregion
    }
}
