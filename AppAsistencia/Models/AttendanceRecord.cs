using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class AttendanceRecord
    {
        #region attributes

        
        [Key]
        [Required]
        public Guid idRecord { get; set; }

        [Required]
        public DateTime captureTimeStamp { get; set; }

        [Required]
        public string captureMethod { get; set; }

        [Required]
        public string attendanceStatus { get; set; }

        [Required]
        public string recordHash { get; set; }

        [Required]
        public bool isActive { get; set; } = true;
        #endregion
        #region relationships

        //this contains the foreign key from Student (1:N) 
      public Guid? studentID { get; set; }
        public Student studentFK { get; set; } = null!;

        //this contains the foreign key from Device (1:N)
      
        public Guid? deviceID { get; set; }
        public Device deviceFK { get; set; } = null!;

        //this contains the foreign key from ClassSession (1:N)
       
        public Guid? classSessionID { get; set; }
        public ClassSession classSessionFK { get; set; } = null!;

        #endregion
    }
}
