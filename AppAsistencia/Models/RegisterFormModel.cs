namespace AppAsistencia.Models
{
    public class RegisterFormModel
    {
        public string UserType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string InstitutionalCode { get; set; } = string.Empty;
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

        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
