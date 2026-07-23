using AppAsistencia.Models;
using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class StudentDTO
    {
        #region attributes


        [Key]
        [Required]
        public Guid idStudent { get; set; }

        [Required]
        public string studentIdCard { get; set; }
        [Required]
        public int currentSemester { get; set; }
        [Required]
        public int phoneNumber { get; set; } = 0;
        #endregion

        #region relationships

        // One-to-Many relationship with AttendanceRecord
        public ICollection<AttendanceRecordDTO> attendanceRecordsDTOFK { get; set; } = new List<AttendanceRecordDTO>();

        // Many-to-Many relationship with Group
        public List<GroupDTO> groupsDTOFK { get; set; } = [];

        //one-to-one relationship with User
        public UserDTO userDTO { get; set; }


        #endregion
    }
}
