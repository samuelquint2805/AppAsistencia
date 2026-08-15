namespace AppAsistencia.DTOs
{
    public class GroupSummaryDTO
    {
        public Guid IdGroup { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Classroom { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int TotalEstudiantes { get; set; }
        public DateTime? ProximaSesion { get; set; }
    }
}
