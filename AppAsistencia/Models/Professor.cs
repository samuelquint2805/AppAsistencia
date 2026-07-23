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
        public string professorIdCard { get; set; }
        [Required]
        public int phoneNumber { get; set; }
        [Required]
        public string department { get; set; }
        #endregion

        #region relationships 
        // One-to-Many relationship with group
        public ICollection<Group> groupsFK { get; set; } = new List<Group>();

        public Guid? userId { get; set; }
        public User user { get; set; }
        #endregion
    }
}
