
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
        public string groupName { get; set; } = null!;

        [Required]
        public string schedule { get; set; } = null!;
        [Required]
        public string classroom { get; set; } = null!;

        [Required]
        public string semester { get; set; } = null!;

        [Required]
        public bool isActive { get; set; } = true;
        #endregion

        #region relationships
        // One-to-Many relationship with student
        public List<StudentGroup> studentGroupsFK { get; set; } = [];

        // One-to-Many relationship with classSession
        public ICollection<ClassSession> classSessionsFK { get; set; } = new List<ClassSession>();

        // One-to-Many relationship with subject
        public Guid? subjectID { get; set; }
        public Subject subjectFK     { get; set; } = null!;

        // One-to-Many relationship with professor
        public Guid? professorID { get; set; }
        public Professor professorFK { get; set; } = null!;
        #endregion
    }
}
