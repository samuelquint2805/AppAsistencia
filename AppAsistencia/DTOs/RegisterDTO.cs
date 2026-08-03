using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.DTOs
{
    public enum RoleType
    {
        Student,
        Professor,
        Administrator
    }
    public class RegisterDTO
    {
        

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$",
     ErrorMessage = "La contraseña debe tener mínimo 6 caracteres, letras, números y un carácter especial")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar el rol del usuario")]
        public RoleType Role { get; set; }

        // --- Datos específicos por rol. Solo se validan los del rol seleccionado ---

        // Requeridos si Role == Student
        public string? studentIdCard { get; set; }
        public int? currentSemester { get; set; }

        // Requeridos si Role == Professor
        public string? professorIdCard { get; set; }
        public string? department { get; set; }

        // Usado por Student, Professor y Administrator
        public string? phoneNumber { get; set; }
    }
}
