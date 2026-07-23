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
        public string professorIdCard { get; set; }
        [Required]
        public int phoneNumber { get; set; }
        [Required]
        public string department { get; set; }
        #endregion

        #region relationships 
        // One-to-Many relationship with group
        public ICollection<GroupDTO> groupsDTOFK { get; set; } = new List<GroupDTO>();

        // One-to-One relationship with User
        public Guid? userDTOId { get; set; }
        public UserDTO userDTOFK { get; set; }
        #endregion

    }
}
