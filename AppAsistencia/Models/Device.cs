using Microsoft.AspNetCore.Components.Routing;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class Device
    {
        #region attributes

        [Key]
        [Required]
        public Guid idDevice { get; set; }
        [Required]
        public string deviceType { get; set; } = null!;

        [Required]
        public string identifier { get; set; } = null!; 
        [Required]
        public string location { get; set; } = null!;
        [Required]
        public bool isActive { get; set; } = true;
        #endregion

        #region relationships
        // One-to-Many relationship with AttendanceRecord
        public ICollection<AttendanceRecord> attendanceRecords { get; set; } = new List<AttendanceRecord>();
        #endregion
    }
}
