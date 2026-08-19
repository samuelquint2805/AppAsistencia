using AppAsistencia.Core;
using AppAsistencia.DTOs;
using static AppAsistencia.DTOs.Classoptionsdto;

namespace AppAsistencia.Services.Abstractions
{
    public interface IClassService
    {
        // Registro de clase (crea o reutiliza Group + crea la ClassSession de esa fecha)
        Task<Response<GroupSummaryDTO>> RegistrarClaseAsync(RegisterClassDTO dto, Guid idProfessor);

        // Inscripcion de estudiantes
        Task<Response<bool>> RegistrarEstudiantesAsync(AddStudentsToGroupDTO dto);
        Task<Response<ImportStudentsResultDTO>> RegistrarEstudiantesDesdeArchivoAsync(Guid idGroup, Stream archivo, string nombreArchivo);

        // Listado y edicion
        Task<Response<List<GroupSummaryDTO>>> ObtenerClasesDelDocenteAsync(Guid idProfessor);
        Task<Response<GroupSummaryDTO>> ObtenerClasePorIdAsync(Guid idGroup);
        Task<Response<bool>> EditarClaseAsync(EditClassDTO dto);
        Task<Response<bool>> DesactivarClaseAsync(Guid idGroup);

        // Datos de apoyo para los formularios
        Task<Response<List<StudentOptionDTO>>> ObtenerTodosLosEstudiantesAsync();
       
    }
}
