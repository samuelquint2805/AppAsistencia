using AppAsistencia.Core;
using AppAsistencia.DTOs;
using AppAsistencia.Models;

namespace AppAsistencia.Services.Abstractions
{
    public interface IUserService
    {
        // Crea el User y, en la misma transacción, la fila del rol correspondiente
        // (Student/Professor/Administrator) reutilizando el mismo Guid como PK compartida.
        Task<Response<UserSummaryDTO>> RegistrarUsuarioAsync(RegisterDTO dto);

        // Valida email + contraseña (BCrypt).
        Task<Response<User>> ValidarCredencialesAsync(string email, string password);

        Task<Response<User>> ObtenerPorEmailAsync(string email);

        // Activa/inactiva la cuenta (soft state, no elimina el registro).
        Task<Response<bool>> CambiarEstadoUsuarioAsync(Guid idUser, bool activar);

        // Verifica que el correo pertenezca al dominio institucional @correo.itm.edu.co
        Task<Response<bool>> ValidarEmailAsync(string email);

        Task<Response<string>> GenerarTokenConfirmacionAsync(Guid idUser);

        Task<Response<bool>> ConfirmarEmailAsync(Guid idUser, string token);
    }
}
