using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class UserDTO
    {
        #region atributes
        [Key]
        [Required]
        public Guid idUser { get; set; }

       

        [Required]
        public string userName { get; set; } = null!; 

        [Required]
        public string firstname { get; set; } = null!;

        [Required]
        public string lastName { get; set; } = null!;

        [Required]
        public string email { get; set; } = null!;

        [Required]
        public string passwordHash { get; set; } = null!;

        [Required]
        public bool isActive { get; set; } = true;

        [Required]
        public bool isEmailConfirmed { get; set; } = false;
        [Required]
        public DateTime registerDate { get; set; }

        [Required]
        public DateTime accountRenewalDate { get; set; }
        #endregion

        #region relationships
        // One-to-Many relationship with Student
       
        public StudentDTO studentDTOFK { get; set; } = null!;

        // One-to-Many relationship with Administrator
       
        public AdministratorDTO administratorDTOFK { get; set; } = null!;

        // One-to-Many relationship with professor
        
        public ProfessorDTO professorDTOFK { get; set; } = null!;

        // One-to-Many relationship with role
        public Guid? roleDTOID { get; set; }
        public RoleDTO roleDTOFK { get; set; } = null!;

        #endregion
    }
}
