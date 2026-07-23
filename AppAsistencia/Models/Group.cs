
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class Group
    {
        #region attributes

            
        [Key]
        [Required]
        public Guid idGroup { get; set; }

        [Required]
        public string groupName { get; set; }

        [Required]
        public string schedule { get; set; }
        [Required]
        public string classroom { get; set; }

        [Required]
        public string semester { get; set; }

        [Required]
        public bool isActive { get; set; } = true;
        #endregion

        #region relationships
        // One-to-Many relationship with student
        public List<Student> studentGroupsFK { get; set; } = [];

        // One-to-Many relationship with classSession
        public ICollection<ClassSession> classSessionsFK { get; set; } = new List<ClassSession>();

        // One-to-Many relationship with subject
        public Guid? subjectID { get; set; }
        public Subject subjectFK     { get; set; }

        // One-to-Many relationship with professor
        public Guid? professorID { get; set; }
        public Professor professorFK { get; set; }
        #endregion
    }
}
