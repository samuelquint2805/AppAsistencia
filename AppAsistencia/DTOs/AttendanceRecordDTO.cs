using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class AttendanceRecordDTO
    {
        #region attributes

        [Key]
        [Required]
        public Guid idRecord { get; set; }

        [Required]
        public DateTime captureTimeStamp { get; set; }

        [Required]
        public string captureMethod { get; set; } = null!;

        [Required]
        public string attendanceStatus { get; set; } = null!;

        [Required]
        public string recordHash { get; set; } = null!;

        [Required]
        public bool isActive { get; set; } = true;
        #endregion
        #region relationships

        //this contains the foreign key from Student (1:N) 
        public Guid? studentDTOID { get; set; }
        public StudentDTO studentDTOFK { get; set; } = null!;

        //this contains the foreign key from Device (1:N)

        public Guid? deviceDTOID { get; set; }
        public DeviceDTO deviceDTOFK { get; set; } = null!;

        //this contains the foreign key from ClassSession (1:N)

        public Guid? classSessionDTOID { get; set; }
        public ClassSessionDTO classSessionDTOFK { get; set; } = null!;

        #endregion
    }
}
