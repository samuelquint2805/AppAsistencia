using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class ScheduleDayDTO
    {
        [Required]
        public DayOfWeek DiaSemana { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }
    }

    public class ProfessorOptionDTO
    {
        public Guid IdTeacher { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateCourseDTO
    {
        [Required(ErrorMessage = "Selecciona una asignatura")]
        public Guid SubjectId { get; set; }

        [Required(ErrorMessage = "El código del curso es obligatorio")]
        public string GroupCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre/identificador del grupo es obligatorio")]
        public string GroupName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El aula es obligatoria")]
        public string Classroom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Selecciona el docente responsable")]
        public Guid ProfessorId { get; set; }

        // Semestre opcional: si no se envía, se calcula automáticamente
        public string? Semester { get; set; }

        [Required(ErrorMessage = "Selecciona al menos un día de clase")]
        [MinLength(1, ErrorMessage = "Selecciona al menos un día de clase")]
        public List<ScheduleDayDTO> Dias { get; set; } = new();
    }

    public class EditCourseDTO
    {
        [Required]
        public Guid IdGroup { get; set; }

        [Required]
        public string GroupCode { get; set; } = string.Empty;

        [Required]
        public string GroupName { get; set; } = string.Empty;

        [Required]
        public string Classroom { get; set; } = string.Empty;

        [Required]
        public Guid ProfessorId { get; set; }

        [Required]
        [MinLength(1)]
        public List<ScheduleDayDTO> Dias { get; set; } = new();
    }

    public class CourseSummaryDTO
    {
        public Guid IdGroup { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string GroupCode { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string ProfessorName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalSesiones { get; set; }

        // "445232 - 05"
        public string EtiquetaDisplay => $"{SubjectCode} - {GroupCode}";

        public List<ScheduleDayDTO> Dias { get; set; } = new();
        public string HorarioResumen { get; set; } = string.Empty;
    }
}
