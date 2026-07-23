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
        [Required(ErrorMessage = "El código institucional es obligatorio")]
        public string InstitutionalCode { get; set; } = string.Empty;

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
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe indicar el rol del usuario")]
        public RoleType Role { get; set; }

        // --- Datos específicos por rol. Solo se validan los del rol seleccionado ---

        // Requeridos si Role == Student
        public string? StudentIdCard { get; set; }
        public int? CurrentSemester { get; set; }

        // Requeridos si Role == Professor
        public string? ProfessorIdCard { get; set; }
        public string? Department { get; set; }

        // Usado por Student, Professor y Administrator
        public int? PhoneNumber { get; set; }
    }
}
