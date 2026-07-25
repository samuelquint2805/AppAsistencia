using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class ClassSessionDTO
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
        public ICollection<AttendanceRecordDTO> attendanceRecordsDTO { get; set; } = new List<AttendanceRecordDTO>();

        // One-to-Many relationship with Group
        public Guid? groupDTOID { get; set; }
        public GroupDTO groupFK { get; set; } = null!;

        #endregion
    }
}
