using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class CreateSubjectDTO
    {
        [Required(ErrorMessage = "El código de la asignatura es obligatorio")]
        public string SubjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la asignatura es obligatorio")]
        public string Name { get; set; } = string.Empty;
    }

    public class EditSubjectDTO
    {
        [Required]
        public Guid IdSubject { get; set; }

        [Required(ErrorMessage = "El código de la asignatura es obligatorio")]
        public string SubjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de la asignatura es obligatorio")]
        public string Name { get; set; } = string.Empty;
    }

    public class SubjectSummaryDTO
    {
        public Guid IdSubject { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalCursos { get; set; }
    }
}
}
