namespace AppAsistencia.Models
{
    // Vista de carga para el rol Student
    public class ConfigurationStudentViewModel
    {
        public Guid IdUser { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string StudentIdCard { get; set; } = string.Empty;
        public int CurrentSemester { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }

    // Vista de carga compartida para Professor y Administrator
    // (ProfessorIdCard/Department quedan null cuando el usuario es Administrator)
    public class ConfigurationStaffViewModel
    {
        public Guid IdUser { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EsProfessor { get; set; }
        public string? ProfessorIdCard { get; set; }
        public string? Department { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }

    // Datos que llegan del formulario al guardar (comun a los tres roles)
    public class ConfigurationFormModel
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? StudentIdCard { get; set; }    // solo aplica a Student
        public int? CurrentSemester { get; set; }     // solo aplica a Student
        public string? ProfessorIdCard { get; set; }  // solo aplica a Professor

        public string? Department { get; set; }        // solo aplica a Professor

        public string? CurrentPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
    }
}