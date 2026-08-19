using AppAsistencia.Core;
using AppAsistencia.Data.DBSET;
using AppAsistencia.DTOs;
using AppAsistencia.Models;
using AppAsistencia.Services.Abstractions;

namespace AppAsistencia.Services.Implementations
{
    public class SubjectService : ISubjectService
    {
        private readonly DataContextAsistencia _context;

        public SubjectService(DataContextAsistencia context)
        {
            _context = context;
        }

        public async Task<Response<SubjectSummaryDTO>> CrearAsignaturaAsync(CreateSubjectDTO dto)
        {
            try
            {
                var codigo = dto.SubjectCode.Trim();

                var yaExiste = await _context.Set<Subject>()
                    .FirstOrDefaultAsync(s => s.subjectCode == codigo);

                if (yaExiste is not null)
                    return Response<SubjectSummaryDTO>.Failure(
                        $"Ya existe una asignatura con el código '{codigo}': {yaExiste.name}");

                var asignatura = new Subject
                {
                    idSubject = Guid.NewGuid(),
                    subjectCode = codigo,
                    name = dto.Name.Trim(),
                    isActive = true
                };

                _context.Add(asignatura);
                await _context.SaveChangesAsync();

                return Response<SubjectSummaryDTO>.Success(new SubjectSummaryDTO
                {
                    IdSubject = asignatura.idSubject,
                    SubjectCode = asignatura.subjectCode,
                    Name = asignatura.name,
                    IsActive = asignatura.isActive,
                    TotalCursos = 0
                }, "Asignatura creada correctamente");
            }
            catch (Exception ex)
            {
                return Response<SubjectSummaryDTO>.Failure(ex, "No se pudo crear la asignatura");
            }
        }

        public async Task<Response<BulkImportResultDTO>> CargarAsignaturasMasivoAsync(Stream archivo, string nombreArchivo)
        {
            var resultado = new BulkImportResultDTO();

            try
            {
                var filas = FileRowParser.ExtraerFilas(archivo, nombreArchivo, resultado.Errores);

                var codigosExistentes = await _context.Set<Subject>()
                    .Select(s => new { s.subjectCode, s.name })
                    .ToListAsync();

                var codigosVistos = codigosExistentes.ToDictionary(s => s.subjectCode, s => s.name, StringComparer.OrdinalIgnoreCase);

                foreach (var fila in filas)
                {
                    var nombre = fila.GetValueOrDefault("name")?.Trim();
                    var codigo = fila.GetValueOrDefault("subjectCode")?.Trim();

                    if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(codigo))
                    {
                        resultado.Errores.Add("Una fila no tiene nombre o código y fue omitida");
                        continue;
                    }

                    if (codigosVistos.TryGetValue(codigo, out var nombreExistente))
                    {
                        resultado.Duplicados.Add(
                            $"La asignatura '{nombre}' (código {codigo}) está duplicada con '{nombreExistente}'");
                        continue;
                    }

                    _context.Add(new Subject
                    {
                        idSubject = Guid.NewGuid(),
                        subjectCode = codigo,
                        name = nombre,
                        isActive = true
                    });

                    codigosVistos[codigo] = nombre; // evita duplicados tambien DENTRO del mismo archivo
                    resultado.Creados++;
                }

                await _context.SaveChangesAsync();
                return Response<BulkImportResultDTO>.Success(resultado, "Carga masiva de asignaturas procesada");
            }
            catch (Exception ex)
            {
                return Response<BulkImportResultDTO>.Failure(ex, "No se pudo procesar el archivo de asignaturas");
            }
        }

        public async Task<Response<List<SubjectSummaryDTO>>> ObtenerTodasAsync()
        {
            try
            {
                var asignaturas = await _context.Set<Subject>()
                    .Include(s => s.groupsFK)
                    .OrderBy(s => s.name)
                    .Select(s => new SubjectSummaryDTO
                    {
                        IdSubject = s.idSubject,
                        SubjectCode = s.subjectCode,
                        Name = s.name,
                        IsActive = s.isActive,
                        TotalCursos = s.groupsFK.Count
                    })
                    .ToListAsync();

                return Response<List<SubjectSummaryDTO>>.Success(asignaturas);
            }
            catch (Exception ex)
            {
                return Response<List<SubjectSummaryDTO>>.Failure(ex, "No se pudieron obtener las asignaturas");
            }
        }

        public async Task<Response<SubjectSummaryDTO>> ObtenerPorIdAsync(Guid idSubject)
        {
            try
            {
                var s = await _context.Set<Subject>()
                    .Include(x => x.groupsFK)
                    .FirstOrDefaultAsync(x => x.idSubject == idSubject);

                if (s is null)
                    return Response<SubjectSummaryDTO>.Failure("La asignatura no existe");

                return Response<SubjectSummaryDTO>.Success(new SubjectSummaryDTO
                {
                    IdSubject = s.idSubject,
                    SubjectCode = s.subjectCode,
                    Name = s.name,
                    IsActive = s.isActive,
                    TotalCursos = s.groupsFK.Count
                });
            }
            catch (Exception ex)
            {
                return Response<SubjectSummaryDTO>.Failure(ex, "No se pudo obtener la asignatura");
            }
        }

        public async Task<Response<bool>> EditarAsignaturaAsync(EditSubjectDTO dto)
        {
            try
            {
                var asignatura = await _context.Set<Subject>().FindAsync(dto.IdSubject);
                if (asignatura is null)
                    return Response<bool>.Failure("La asignatura no existe");

                var codigo = dto.SubjectCode.Trim();

                var otraConMismoCodigo = await _context.Set<Subject>()
                    .FirstOrDefaultAsync(s => s.subjectCode == codigo && s.idSubject != dto.IdSubject);

                if (otraConMismoCodigo is not null)
                    return Response<bool>.Failure(
                        $"Ya existe otra asignatura con el código '{codigo}': {otraConMismoCodigo.name}");

                asignatura.subjectCode = codigo;
                asignatura.name = dto.Name.Trim();

                await _context.SaveChangesAsync();
                return Response<bool>.Success(true, "Asignatura actualizada correctamente");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudo actualizar la asignatura");
            }
        }

        public async Task<Response<bool>> DeshabilitarAsignaturaAsync(Guid idSubject)
        {
            return await CambiarEstadoAsync(idSubject, false);
        }

        public async Task<Response<bool>> HabilitarAsignaturaAsync(Guid idSubject)
        {
            return await CambiarEstadoAsync(idSubject, true);
        }

        public async Task<Response<bool>> EliminarAsignaturaAsync(Guid idSubject)
        {
            try
            {
                var asignatura = await _context.Set<Subject>()
                    .Include(s => s.groupsFK)
                    .FirstOrDefaultAsync(s => s.idSubject == idSubject);

                if (asignatura is null)
                    return Response<bool>.Failure("La asignatura no existe");

                if (asignatura.groupsFK.Count > 0)
                    return Response<bool>.Failure(
                        $"No se puede eliminar: tiene {asignatura.groupsFK.Count} curso(s) asignado(s). Desactívala en su lugar.");

                _context.Remove(asignatura);
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, "Asignatura eliminada correctamente");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudo eliminar la asignatura");
            }
        }

        private async Task<Response<bool>> CambiarEstadoAsync(Guid idSubject, bool activar)
        {
            try
            {
                var asignatura = await _context.Set<Subject>().FindAsync(idSubject);
                if (asignatura is null)
                    return Response<bool>.Failure("La asignatura no existe");

                asignatura.isActive = activar;
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, activar ? "Asignatura habilitada" : "Asignatura deshabilitada");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudo cambiar el estado de la asignatura");
            }
        }
    }
}
