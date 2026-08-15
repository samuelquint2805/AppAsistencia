namespace AppAsistencia.DTOs
{
    public class AddStudentsToGroupDTO
    {
        public Guid IdGroup { get; set; }
        public List<Guid> StudentIds { get; set; } = new();
    }
}
