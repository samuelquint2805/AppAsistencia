using System.ComponentModel.DataAnnotations;

namespace AppAsistencia.Models
{
    public class RegisterFormModel
    {
        
        public string Name { get; set; } = string.Empty;
       
        public string Email { get; set; } = string.Empty;

        // Campos de estudiante
        public string? StudentIdCard { get; set; }
        public int? CurrentSemester { get; set; }

        // Campos de docente
        public string? TeacherCard { get; set; }
        public string? TeacherPhone { get; set; }
        public string? Department { get; set; }

        // Compartido: en el HTML actual, ambos roles usan name="phoneNumber"
        public string? phoneNumber { get; set; }

        [Required]
        [RegularExpression(
    @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).{6,}$",
    ErrorMessage = "La contraseña debe tener mínimo 6 caracteres, e incluir letras, números y un carácter especial")]
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
