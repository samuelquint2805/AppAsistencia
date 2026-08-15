using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class GroupSchedule
    {
        #region attributes
        [Key]
        [Required]
        public Guid idGroupSchedule { get; set; }

        [Required]
        public DayOfWeek dayOfWeek { get; set; }

        [Required]
        public TimeSpan startTime { get; set; }

        [Required]
        public TimeSpan endTime { get; set; }
        #endregion

        #region relationships
        [Required]
        public Guid idGroup { get; set; }
        public Group groupFK { get; set; } = null!;
        #endregion
    }
}
