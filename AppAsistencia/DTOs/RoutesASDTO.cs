using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class RoutesASDTO
    {
        #region atributtes 
        [Key]
        [Required]
        public Guid? idRoute { get; set; }

        [Required]
        public string routeName { get; set; } = null!;
        #endregion

        #region relationships

        // One-to-Many relationship with RouteRole
        public List<RouteRoleDTO> routeRolesDTO { get; } = [];
        #endregion
    }
}
