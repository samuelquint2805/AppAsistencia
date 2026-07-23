using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class Role
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

        public List<RouteRole> routeRoles { get; } = [];

        // One-to-Many relationship with User
        public ICollection<User> usersFK { get; set; } = new List<User>();
        #endregion
    }
}
