using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class AdministratorDTO
    {

        #region attributes


        [Key]
        [Required]
        public Guid idAdmin { get; set; }

        [Required]
        public int phoneMumber { get; set; }
        #endregion

        #region relationships
        //this contains the foreign key from AdminParameter (1:N) to indicate the relationShip between ParametersManagement and AdminParameter (N:M)

        public List<AdminParameterDTO>? ParameterAdmins { get; } = [];

        //one-to-one relationship with User
        public UserDTO UserDTO { get; set; }
        #endregion
    }
}
