using AppAsistencia.Core;
using AppAsistencia.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace AppAsistencia.Services.Implementations
{
    public class TwoFactorService : ITwoFactorService
    {
        // Guarda el codigo en memoria del servidor (no en la base de datos), asociado al idUser.
        // Nota: si en el futuro despliegas en varias instancias/servidores (load balancer),
        // IMemoryCache no se comparte entre ellas. En ese caso habria que migrar a una
        // tabla de BD o a un cache distribuido (ej. Redis). Para un solo servidor (caso tipico
        // de una app universitaria) esto es suficiente y no requiere migraciones nuevas.
        
            private readonly IMemoryCache _cache;
            private readonly IEmailSenderService _emailSender;
            private static readonly TimeSpan Expiracion = TimeSpan.FromMinutes(10);
            private const int MaxIntentos = 5;

            public TwoFactorService(IMemoryCache cache, IEmailSenderService emailSender)
            {
                _cache = cache;
                _emailSender = emailSender;
            }

            public async Task<Response<bool>> GenerarYEnviarCodigoAsync(Guid idUser, string email, string nombre)
            {
                try
                {
                    var codigo = Random.Shared.Next(0, 1_000_000).ToString("D6");

                    var entrada = new CodigoEntry { Codigo = codigo, Intentos = 0 };
                    _cache.Set(ClaveCache(idUser), entrada, Expiracion);

                    await _emailSender.SendEmailAsync(
                        email,
                        "Tu código de verificación - AppAsistencia",
                        $"<p>Hola {nombre}, tu código de verificación es:</p>" +
                        $"<h2 style='letter-spacing:4px'>{codigo}</h2>" +
                        $"<p>Este código expira en 10 minutos. Si no solicitaste este código, ignora este mensaje.</p>");

                    return Response<bool>.Success(true, "Código enviado a tu correo institucional");
                }
                catch (Exception ex)
                {
                    return Response<bool>.Failure(ex, "No se pudo enviar el código de verificación");
                }
            }

            public Response<bool> ValidarCodigo(Guid idUser, string codigo)
            {
                var clave = ClaveCache(idUser);

                if (!_cache.TryGetValue(clave, out CodigoEntry? entrada) || entrada is null)
                    return Response<bool>.Failure("El código expiró o no existe. Solicita uno nuevo.");

                if (entrada.Intentos >= MaxIntentos)
                {
                    _cache.Remove(clave);
                    return Response<bool>.Failure("Demasiados intentos fallidos. Solicita un nuevo código.");
                }

                if (entrada.Codigo != codigo)
                {
                    entrada.Intentos++;
                    _cache.Set(clave, entrada, Expiracion);
                    return Response<bool>.Failure("Código incorrecto");
                }

                _cache.Remove(clave);
                return Response<bool>.Success(true, "Código verificado correctamente");
            }

            private static string ClaveCache(Guid idUser) => $"2fa:{idUser}";

            private class CodigoEntry
            {
                public string Codigo { get; set; } = string.Empty;
                public int Intentos { get; set; }
            }
        }
    }

