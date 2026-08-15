using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class RegisterClassDTO
    {
        [Required(ErrorMessage = "Selecciona un curso")]
        public Guid SubjectId { get; set; }

        [Required(ErrorMessage = "El nombre del grupo es obligatorio")]
        public string GroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El aula es obligatoria")]
        public string Classroom { get; set; } = string.Empty;

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime SessionDate { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria")]
        public TimeSpan EndTime { get; set; }
    }
}
