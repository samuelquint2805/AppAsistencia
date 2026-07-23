using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class SubjectDTO
    {
        #region attributes
        [Key]
        [Required]
        public Guid idSubject { get; set; }

        [Required]
        public string subjectCode { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public int credits { get; set; }
        [Required]
        public bool isActive { get; set; } = true;
        #endregion

        #region relationships

        // One-to-Many relationship with group
        public ICollection<GroupDTO> groupsDTOFK { get; set; } = new List<GroupDTO>();


        #endregion
    }
}
