namespace AppAsistencia.Services.Abstractions
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
