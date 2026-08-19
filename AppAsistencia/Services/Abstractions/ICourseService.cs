using AppAsistencia.Core;
using AppAsistencia.DTOs;

namespace AppAsistencia.Services.Abstractions
{
    public interface ICourseService
    {
        Task<Response<CourseSummaryDTO>> CrearCursoAsync(CreateCourseDTO dto);
        Task<Response<BulkImportResultDTO>> CargarCursosMasivoAsync(Stream archivo, string nombreArchivo);

        Task<Response<List<CourseSummaryDTO>>> ObtenerTodosAsync();
        Task<Response<CourseSummaryDTO>> ObtenerPorIdAsync(Guid idGroup);
        Task<Response<List<CourseSummaryDTO>>> ObtenerCursosDelDocenteAsync(Guid idProfessor);

        Task<Response<bool>> EditarCursoAsync(EditCourseDTO dto);
        Task<Response<bool>> DeshabilitarCursoAsync(Guid idGroup);
        Task<Response<bool>> HabilitarCursoAsync(Guid idGroup);
        Task<Response<bool>> EliminarCursoAsync(Guid idGroup);
        Task<Response<List<ProfessorOptionDTO>>> ObtenerProfesoresDisponiblesAsync();
    }
}
