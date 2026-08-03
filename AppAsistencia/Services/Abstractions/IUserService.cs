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

        Task<Response<bool>> ConfirmarEmailDirectoAsync(Guid idUser);
        Task<Response<User>> ObtenerPorIdAsync(Guid idUser);

        // Activa/inactiva la cuenta (soft state, no elimina el registro).
        Task<Response<bool>> CambiarEstadoUsuarioAsync(Guid idUser, bool activar);

        // Verifica que el correo pertenezca al dominio institucional @correo.itm.edu.co o @itm.edu.co
        Task<Response<bool>> ValidarEmailAsync(string email);

        Task<Response<string>> GenerarTokenConfirmacionAsync(Guid idUser);

        Task<Response<bool>> ConfirmarEmailAsync(Guid idUser, string token);
        Task<Response<bool>> ActualizarPerfilAsync(Guid idUser, string firstName, string lastName, string email);
        Task<Response<bool>> CambiarContrasenaAsync(Guid idUser, string actual, string nueva);
        Task<Response<Student>> ObtenerEstudiantePorIdAsync(Guid idUser);
        Task<Response<Professor>> ObtenerProfesorPorIdAsync(Guid idUser);
        Task<Response<Administrator>> ObtenerAdministradorPorIdAsync(Guid idUser);
        Task<Response<bool>> ActualizarSemestreAsync(Guid idUser, int nuevoSemestre);
        Task<Response<bool>> ActualizarDatosEstudianteAsync(Guid idUser, string? phoneNumber, int currentSemester, string? studentIdCard);
        Task<Response<bool>> ActualizarDatosProfesorAsync(Guid idUser, string? phoneNumber, string? department, string? professorIdCard);
        Task<Response<bool>> ActualizarDatosAdministradorAsync(Guid idUser, string? phoneNumber);

    }
}
