using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class VerifyCodeDTO
    {
        [Required]
        public Guid IdUser { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "El código debe tener 6 dígitos")]
        public string Codigo { get; set; } = string.Empty;
    }
}
