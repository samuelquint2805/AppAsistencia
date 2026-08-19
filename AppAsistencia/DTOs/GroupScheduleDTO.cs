using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class GroupScheduleDTO
    {
        [Required]
        public Guid idGroupSchedule { get; set; }

        [Required]
        public DayOfWeek dayOfWeek { get; set; }

        [Required]
        public TimeSpan startTime { get; set; }

        [Required]
        public TimeSpan endTime { get; set; }


       
        [Required]
        public Guid idGroup { get; set; }
        public GroupDTO groupDTOFK { get; set; } = null!;
    }
}
