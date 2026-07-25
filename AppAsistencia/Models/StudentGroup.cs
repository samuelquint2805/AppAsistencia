using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class StudentGroup
    {
        #region attributes
        [Required]
        public string enrollmentDate { get; set; } = null!;
        #endregion

        #region relationships 
        // One-to-Many relationship with Student
        public Guid? studentID { get; set; }
        public Student studentFK { get; set; } = null!;

        // One-to-Many relationship with Group
        public Guid? GroupID { get; set; }
        public Group groupFK { get; set; } = null!;
        #endregion

    }
}
