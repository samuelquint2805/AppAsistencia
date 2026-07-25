using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class ProfessorDTO
    {
        #region attributes
        [Key]
        [Required]
        public Guid idTeacher { get; set; }

        [Required]
        public string professorIdCard { get; set; } = string.Empty;
        [Required]
        public string phoneNumber { get; set; } = null!;
        [Required]
        public string department { get; set; } = null!;
        #endregion

        #region relationships 
        // One-to-Many relationship with group
        public ICollection<GroupDTO> groupsDTOFK { get; set; } = new List<GroupDTO>();

        // One-to-One relationship with User
        
        public UserDTO userDTOFK { get; set; } = null!;
        #endregion

    }
}
