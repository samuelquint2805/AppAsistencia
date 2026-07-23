namespace AppAsistencia.DTOs
{
    public class UserSummaryDTO
    {
        public Guid IdUser { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; }
    }
}
