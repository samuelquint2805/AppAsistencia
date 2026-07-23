using AppAsistencia.Models;
using Microsoft.AspNetCore.Components.Routing;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs

{
    public class DeviceDTO
    {
        #region attributes

        [Key]
        [Required]
        public Guid idDevice { get; set; }
        [Required]
        public string deviceType { get; set; }

        [Required]
        public string identifier { get; set; }
        [Required]
        public string location { get; set; }
        [Required]
        public bool isActive { get; set; } = true;
        #endregion

        #region relationships
        // One-to-Many relationship with AttendanceRecord
        public ICollection<AttendanceRecordDTO> attendanceRecordsDTO { get; set; } = new List<AttendanceRecordDTO>();
        #endregion

    }
}
