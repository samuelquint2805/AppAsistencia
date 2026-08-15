namespace AppAsistencia.DTOs
{
    public class Classoptionsdto
    {
        public class StudentOptionDTO
        {
            public Guid IdStudent { get; set; }
            public string NombreCompleto { get; set; } = string.Empty;
            public string StudentIdCard { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class SubjectOptionDTO
        {
            public Guid IdSubject { get; set; }
            public string SubjectCode { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }
    }
}
