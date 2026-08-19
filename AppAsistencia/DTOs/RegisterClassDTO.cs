using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class RegisterClassDTO
    {
        [Required(ErrorMessage = "Selecciona el curso")]
        public Guid IdGroup { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime SessionDate { get; set; }
    }
}
