using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class RoleDTO
    {

        #region attributes
        [Key]
        [Required]
        public Guid idRol { get; set; }

        [Required]
        public string nombreRol { get; set; }
        [Required]
        public string descripcion { get; set; }
        [Required]
        public string rutaPermisos { get; set; }
        #endregion

        #region relationships
        
        public List<RouteRoleDTO> routeRolesDTO { get; } = [];

        // One-to-Many relationship with User
        public ICollection<UserDTO> usersDTOFK { get; set; } = new List<UserDTO>();
        #endregion

    }
}
