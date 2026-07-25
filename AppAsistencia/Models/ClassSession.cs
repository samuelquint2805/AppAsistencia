using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class ClassSession
    {
        #region attributes

        
        [Key]
        [Required]
        public Guid idSession { get; set; }

        [Required]
        public DateTime startTime { get; set; }
      
        [Required]
        public DateTime endTime { get; set; }
      
        [Required]
        public string status { get; set; } = null!;
        #endregion

        #region relationships

        // One-to-Many relationship with AttendanceRecord
        public ICollection<AttendanceRecord> attendanceRecords { get; set; } = new List<AttendanceRecord>();
        
        // One-to-Many relationship with Group
        public Guid? groupID { get; set; } 
        public Group groupFK { get; set; } = null!;

        #endregion
    }
}
