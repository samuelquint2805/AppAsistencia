namespace AppAsistencia.DTOs
{
    public class TwoFactorRequiredDTO
    {
        public Guid IdUser { get; set; }
        public string EmailEnmascarado { get; set; } = string.Empty;
    }
}
