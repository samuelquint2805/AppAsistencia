using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class Professor
    {
        #region attributes
        [Key]
        [Required]
        public Guid idTeacher { get; set; }

        [Required]
        public string professorIdCard { get; set; } = null!;
        [Required]
        public string phoneNumber { get; set; } = null!;
        [Required]
        public string department { get; set; } = null!;
        #endregion

        #region relationships 
        // One-to-Many relationship with group
        public ICollection<Group> groupsFK { get; set; } = new List<Group>();

        public User user { get; set; } = null!;
        #endregion
    }
}
