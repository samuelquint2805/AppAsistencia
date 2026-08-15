using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class EditClassDTO
    {
        [Required]
        public Guid IdGroup { get; set; }

        [Required(ErrorMessage = "El nombre del grupo es obligatorio")]
        public string GroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El aula es obligatoria")]
        public string Classroom { get; set; } = string.Empty;

        // Identifica CUAL sesion se está reprogramando (un Group puede tener varias).
        // Si no se envía, se asume que se edita la próxima sesión pendiente.
        public Guid? IdSession { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime SessionDate { get; set; }

        [Required(ErrorMessage = "La hora de inicio es obligatoria")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "La hora de fin es obligatoria")]
        public TimeSpan EndTime { get; set; }
    }
}
