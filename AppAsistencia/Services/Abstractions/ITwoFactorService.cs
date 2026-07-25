using AppAsistencia.Core;

namespace AppAsistencia.Services.Abstractions
{
    public interface ITwoFactorService
    {

        // Genera un codigo de 6 digitos, lo guarda temporalmente (10 min) y lo envia por correo
        Task<Response<bool>> GenerarYEnviarCodigoAsync(Guid idUser, string email, string nombre);

        // Valida el codigo ingresado por el usuario contra el guardado en cache
        Response<bool> ValidarCodigo(Guid idUser, string codigo);
    }
}
