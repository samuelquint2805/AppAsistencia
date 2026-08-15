using AppAsistencia.Core;
using AppAsistencia.DTOs;

namespace AppAsistencia.Services.Abstractions
{
    public interface ISubjectService
    {
        Task<Response<SubjectSummaryDTO>> CrearAsignaturaAsync(CreateSubjectDTO dto);
        Task<Response<BulkImportResultDTO>> CargarAsignaturasMasivoAsync(Stream archivo, string nombreArchivo);

        Task<Response<List<SubjectSummaryDTO>>> ObtenerTodasAsync();
        Task<Response<SubjectSummaryDTO>> ObtenerPorIdAsync(Guid idSubject);
        Task<Response<bool>> EditarAsignaturaAsync(EditSubjectDTO dto);
        Task<Response<bool>> DeshabilitarAsignaturaAsync(Guid idSubject);
        Task<Response<bool>> HabilitarAsignaturaAsync(Guid idSubject);
        Task<Response<bool>> EliminarAsignaturaAsync(Guid idSubject);
    }
}
