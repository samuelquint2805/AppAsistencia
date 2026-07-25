using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class Student
    {
        #region attributes

        
        [Key]
        [Required]
        public Guid idStudent { get; set; }
    
        [Required]
        public string studentIdCard { get; set; } = null!;
        [Required]
        public int currentSemester { get; set; }
        [Required]
        public string phoneNumber { get; set; } = null!;
        #endregion

        #region relationships

        // One-to-Many relationship with AttendanceRecord
        public ICollection<AttendanceRecord> attendanceRecordsFK { get; set; } = new List<AttendanceRecord>();

        // Many-to-Many relationship with Group
        public List<StudentGroup> groupsFK { get; set; } = [];

        //one-to-one relationship with User
        public User user { get; set; } = null!;


        #endregion
    }
}
