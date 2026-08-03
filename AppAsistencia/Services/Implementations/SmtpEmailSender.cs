using System.Net;
using System.Net.Mail;
using AppAsistencia.Services.Abstractions;
using Microsoft.Extensions.Configuration;

namespace AppAsistencia.Services.Implementations
{
    // Requiere en appsettings.json:
    // "SmtpSettings": {
    //   "Host": "smtp.tuservidor.com",
    //   "Port": "587",
    //   "User": "notificaciones@itm.edu.co",
    //   "Password": "TU_PASSWORD_O_APP_PASSWORD",
    //   "From": "notificaciones@itm.edu.co",
    //   "FromName": "AppAsistencia ITM",
    //   "EnableSsl": "true"
    // }
    public class SmtpEmailSender : IEmailSenderService
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtp = _configuration.GetSection("SmtpSettings");

            var host = smtp["Host"] ?? throw new InvalidOperationException("SmtpSettings:Host no está configurado");
            var port = int.Parse(smtp["Port"] ?? "587");
            var user = smtp["User"] ?? throw new InvalidOperationException("SmtpSettings:User no está configurado");
            var password = smtp["Password"] ?? throw new InvalidOperationException("SmtpSettings:Password no está configurado");
            var fromEmail = smtp["From"] ?? user;
            var fromName = smtp["FromName"] ?? "AppAsistencia (Notificaciones)";
            var enableSsl = bool.Parse(smtp["EnableSsl"] ?? "true");

            using var mensaje = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mensaje.To.Add(toEmail);
            mensaje.Headers.Add("X-Mailer", "AppAsistencia ITM");
            mensaje.Headers.Add("Precedence", "bulk"); // O quita si es correo individual


            using var cliente = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = enableSsl
            };

            try
            {
                await cliente.SendMailAsync(mensaje);
                System.Diagnostics.Debug.WriteLine("✅ CONEXIÓN SMTP EXITOSA: Correo enviado a " + toEmail);
            }
            catch (SmtpException ex)
            {
                // Esto te dirá exactamente el código de respuesta del servidor (ej. 535 Authentication failed)
                System.Diagnostics.Debug.WriteLine($"❌ ERROR DE CONEXIÓN SMTP: {ex.StatusCode} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"DETALLE INTERNO: {ex.InnerException.Message}");
                }
                throw; // Vuelve a lanzar para no ocultar la excepción
            }
        }
    }
}