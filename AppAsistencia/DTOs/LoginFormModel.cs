using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public class LoginFormModel
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo no válido")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
