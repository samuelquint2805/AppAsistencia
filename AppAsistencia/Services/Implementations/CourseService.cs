using AppAsistencia.Core;
using AppAsistencia.Data.DBSET;
using AppAsistencia.DTOs;
using AppAsistencia.Models;
using AppAsistencia.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AppAsistencia.Services.Implementations
{
    public class CourseService : ICourseService
    {
        private readonly DataContextAsistencia _context;

        public CourseService(DataContextAsistencia context)
        {
            _context = context;
        }

        public async Task<Response<CourseSummaryDTO>> CrearCursoAsync(CreateCourseDTO dto)
        {
            try
            {
                var asignatura = await _context.Set<Subject>().FindAsync(dto.SubjectId);
                if (asignatura is null)
                    return Response<CourseSummaryDTO>.Failure("La asignatura seleccionada no existe");

                var profesor = await _context.Set<Professor>()
                    .Include(p => p.user)
                    .FirstOrDefaultAsync(p => p.idTeacher == dto.ProfessorId);

                if (profesor is null)
                    return Response<CourseSummaryDTO>.Failure("El docente seleccionado no existe");

                var codigoGrupo = dto.GroupCode.Trim();
                var semestre = string.IsNullOrWhiteSpace(dto.Semester) ? CalcularSemestreActual() : dto.Semester!.Trim();

                var yaExiste = await _context.Set<Group>()
                    .AnyAsync(g => g.subjectID == dto.SubjectId && g.GroupCode == codigoGrupo && g.semester == semestre);

                if (yaExiste)
                    return Response<CourseSummaryDTO>.Failure(
                        $"Ya existe el curso '{asignatura.subjectCode} - {codigoGrupo}' en el semestre {semestre}");

                foreach (var dia in dto.Dias)
                {
                    if (dia.HoraFin <= dia.HoraInicio)
                        return Response<CourseSummaryDTO>.Failure(
                            $"En {FileRowParser.NombreDiaEnEspanol(dia.DiaSemana)}: la hora de fin debe ser posterior a la de inicio");
                }

                // Choque de aula contra otros cursos activos del mismo semestre
                foreach (var dia in dto.Dias)
                {
                    var conflicto = await _context.Set<GroupSchedule>()
                        .Include(gs => gs.groupFK)
                        .AnyAsync(gs => gs.groupFK.classroom == dto.Classroom
                                     && gs.groupFK.semester == semestre
                                     && gs.groupFK.isActive
                                     && gs.dayOfWeek == dia.DiaSemana
                                     && gs.startTime < dia.HoraFin
                                     && gs.endTime > dia.HoraInicio);

                    if (conflicto)
                        return Response<CourseSummaryDTO>.Failure(
                            $"Choque de horario: el aula {dto.Classroom} ya está ocupada el " +
                            $"{FileRowParser.NombreDiaEnEspanol(dia.DiaSemana)} en ese rango de horas.");
                }

                await using var transaction = await _context.Database.BeginTransactionAsync();

                var grupo = new Group
                {
                    idGroup = Guid.NewGuid(),
                    groupName = dto.GroupName.Trim(),
                    GroupCode = codigoGrupo,
                    classroom = dto.Classroom.Trim(),
                    semester = semestre,
                    isActive = true,
                    subjectID = dto.SubjectId,
                    professorID = dto.ProfessorId
                };
                _context.Add(grupo);

                foreach (var dia in dto.Dias)
                {
                    _context.Add(new GroupSchedule
                    {
                        idGroupSchedule = Guid.NewGuid(),
                        idGroup = grupo.idGroup,
                        dayOfWeek = dia.DiaSemana,
                        startTime = dia.HoraInicio,
                        endTime = dia.HoraFin
                    });
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var resumen = await ObtenerPorIdAsync(grupo.idGroup);
                return Response<CourseSummaryDTO>.Success(resumen.Result!, "Curso creado correctamente");
            }
            catch (Exception ex)
            {
                return Response<CourseSummaryDTO>.Failure(ex, "No se pudo crear el curso");
            }
        }

        public async Task<Response<BulkImportResultDTO>> CargarCursosMasivoAsync(Stream archivo, string nombreArchivo)
        {
            var resultado = new BulkImportResultDTO();

            try
            {
                var filas = FileRowParser.ExtraerFilas(archivo, nombreArchivo, resultado.Errores);

                var asignaturas = await _context.Set<Subject>().ToListAsync();
                var profesores = await _context.Set<Professor>().Include(p => p.user).ToListAsync();

                var combosVistos = new HashSet<string>(
                    await _context.Set<Group>()
                        .Select(g => g.subjectID + "|" + g.GroupCode + "|" + g.semester)
                        .ToListAsync());

                foreach (var fila in filas)
                {
                    try
                    {
                        var subjectCode = fila.GetValueOrDefault("subjectCode")?.Trim();
                        var groupCode = fila.GetValueOrDefault("groupCode")?.Trim();
                        var groupName = fila.GetValueOrDefault("groupName")?.Trim();
                        var classroom = fila.GetValueOrDefault("classroom")?.Trim();
                        var professorEmail = fila.GetValueOrDefault("professorEmail")?.Trim();
                        var semestreFila = fila.GetValueOrDefault("semester")?.Trim();
                        var diasTexto = fila.GetValueOrDefault("days")?.Trim();
                        var horaInicioTexto = fila.GetValueOrDefault("startTime")?.Trim();
                        var horaFinTexto = fila.GetValueOrDefault("endTime")?.Trim();

                        if (string.IsNullOrWhiteSpace(subjectCode) || string.IsNullOrWhiteSpace(groupCode) ||
                            string.IsNullOrWhiteSpace(classroom) || string.IsNullOrWhiteSpace(professorEmail) ||
                            string.IsNullOrWhiteSpace(diasTexto) || string.IsNullOrWhiteSpace(horaInicioTexto) ||
                            string.IsNullOrWhiteSpace(horaFinTexto))
                        {
                            resultado.Errores.Add($"Fila con curso '{subjectCode} - {groupCode}': faltan datos obligatorios");
                            continue;
                        }

                        var asignatura = asignaturas.FirstOrDefault(s => s.subjectCode == subjectCode);
                        if (asignatura is null)
                        {
                            resultado.Errores.Add($"No existe la asignatura con código '{subjectCode}' (curso '{groupCode}')");
                            continue;
                        }

                        var profesor = profesores.FirstOrDefault(p =>
                            p.user.email.Equals(professorEmail, StringComparison.OrdinalIgnoreCase));

                        if (profesor is null)
                        {
                            resultado.Errores.Add($"No existe un docente con correo '{professorEmail}' (curso '{groupCode}')");
                            continue;
                        }

                        var semestre = string.IsNullOrWhiteSpace(semestreFila) ? CalcularSemestreActual() : semestreFila;
                        var clave = $"{asignatura.idSubject}|{groupCode}|{semestre}";

                        if (combosVistos.Contains(clave))
                        {
                            resultado.Duplicados.Add($"El curso '{subjectCode} - {groupCode}' ({semestre}) está duplicado");
                            continue;
                        }

                        if (!TimeSpan.TryParse(horaInicioTexto, out var horaInicio) ||
                            !TimeSpan.TryParse(horaFinTexto, out var horaFin))
                        {
                            resultado.Errores.Add($"Formato de hora inválido en el curso '{groupCode}'");
                            continue;
                        }

                        if (horaFin <= horaInicio)
                        {
                            resultado.Errores.Add($"La hora de fin debe ser posterior a la de inicio en el curso '{groupCode}'");
                            continue;
                        }

                        var nombresDias = diasTexto.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        var diasParseados = new List<DayOfWeek>();

                        foreach (var nombreDia in nombresDias)
                        {
                            var dia = FileRowParser.ParsearDiaSemana(nombreDia);
                            if (dia is null)
                            {
                                resultado.Errores.Add($"Día no reconocido '{nombreDia}' en el curso '{groupCode}'");
                                continue;
                            }
                            diasParseados.Add(dia.Value);
                        }

                        if (diasParseados.Count == 0)
                        {
                            resultado.Errores.Add($"El curso '{groupCode}' no tiene días válidos");
                            continue;
                        }

                        var grupo = new Group
                        {
                            idGroup = Guid.NewGuid(),
                            groupName = string.IsNullOrWhiteSpace(groupName) ? groupCode : groupName,
                            GroupCode = groupCode,
                            classroom = classroom,
                            semester = semestre,
                            isActive = true,
                            subjectID = asignatura.idSubject,
                            professorID = profesor.idTeacher
                        };
                        _context.Add(grupo);

                        foreach (var dia in diasParseados)
                        {
                            _context.Add(new GroupSchedule
                            {
                                idGroupSchedule = Guid.NewGuid(),
                                idGroup = grupo.idGroup,
                                dayOfWeek = dia,
                                startTime = horaInicio,
                                endTime = horaFin
                            });
                        }

                        combosVistos.Add(clave);
                        resultado.Creados++;
                    }
                    catch (Exception exFila)
                    {
                        resultado.Errores.Add($"Error inesperado procesando una fila: {exFila.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                return Response<BulkImportResultDTO>.Success(resultado, "Carga masiva de cursos procesada");
            }
            catch (Exception ex)
            {
                return Response<BulkImportResultDTO>.Failure(ex, "No se pudo procesar el archivo de cursos");
            }
        }

        public async Task<Response<List<CourseSummaryDTO>>> ObtenerTodosAsync()
        {
            try
            {
                var grupos = await _context.Set<Group>()
                    .Include(g => g.subjectFK)
                    .Include(g => g.professorFK).ThenInclude(p => p.user)
                    .Include(g => g.scheduleDaysFK)
                    .Include(g => g.classSessionsFK)
                    .OrderBy(g => g.subjectFK.subjectCode).ThenBy(g => g.GroupCode)
                    .ToListAsync();

                return Response<List<CourseSummaryDTO>>.Success(grupos.Select(MapearResumen).ToList());
            }
            catch (Exception ex)
            {
                return Response<List<CourseSummaryDTO>>.Failure(ex, "No se pudieron obtener los cursos");
            }
        }

        public async Task<Response<CourseSummaryDTO>> ObtenerPorIdAsync(Guid idGroup)
        {
            try
            {
                var g = await _context.Set<Group>()
                    .Include(x => x.subjectFK)
                    .Include(x => x.professorFK).ThenInclude(p => p.user)
                    .Include(x => x.scheduleDaysFK)
                    .Include(x => x.classSessionsFK)
                    .FirstOrDefaultAsync(x => x.idGroup == idGroup);

                if (g is null)
                    return Response<CourseSummaryDTO>.Failure("El curso no existe");

                return Response<CourseSummaryDTO>.Success(MapearResumen(g));
            }
            catch (Exception ex)
            {
                return Response<CourseSummaryDTO>.Failure(ex, "No se pudo obtener el curso");
            }
        }

        public async Task<Response<List<CourseSummaryDTO>>> ObtenerCursosDelDocenteAsync(Guid idProfessor)
        {
            try
            {
                var grupos = await _context.Set<Group>()
                    .Include(g => g.subjectFK)
                    .Include(g => g.professorFK).ThenInclude(p => p.user)
                    .Include(g => g.scheduleDaysFK)
                    .Include(g => g.classSessionsFK)
                    .Where(g => g.professorID == idProfessor && g.isActive)
                    .OrderBy(g => g.subjectFK.subjectCode)
                    .ToListAsync();

                return Response<List<CourseSummaryDTO>>.Success(grupos.Select(MapearResumen).ToList());
            }
            catch (Exception ex)
            {
                return Response<List<CourseSummaryDTO>>.Failure(ex, "No se pudieron obtener los cursos del docente");
            }
        }

        public async Task<Response<bool>> EditarCursoAsync(EditCourseDTO dto)
        {
            try
            {
                var grupo = await _context.Set<Group>()
                    .Include(g => g.scheduleDaysFK)
                    .FirstOrDefaultAsync(g => g.idGroup == dto.IdGroup);

                if (grupo is null)
                    return Response<bool>.Failure("El curso no existe");

                var codigoGrupo = dto.GroupCode.Trim();

                var duplicado = await _context.Set<Group>()
                    .AnyAsync(g => g.idGroup != dto.IdGroup && g.subjectID == grupo.subjectID
                                 && g.GroupCode == codigoGrupo && g.semester == grupo.semester);

                if (duplicado)
                    return Response<bool>.Failure($"Ya existe otro curso con el código '{codigoGrupo}' en este semestre");

                foreach (var dia in dto.Dias)
                {
                    if (dia.HoraFin <= dia.HoraInicio)
                        return Response<bool>.Failure(
                            $"En {FileRowParser.NombreDiaEnEspanol(dia.DiaSemana)}: la hora de fin debe ser posterior a la de inicio");
                }

                foreach (var dia in dto.Dias)
                {
                    var conflicto = await _context.Set<GroupSchedule>()
                        .Include(gs => gs.groupFK)
                        .AnyAsync(gs => gs.idGroup != dto.IdGroup
                                     && gs.groupFK.classroom == dto.Classroom
                                     && gs.groupFK.semester == grupo.semester
                                     && gs.groupFK.isActive
                                     && gs.dayOfWeek == dia.DiaSemana
                                     && gs.startTime < dia.HoraFin
                                     && gs.endTime > dia.HoraInicio);

                    if (conflicto)
                        return Response<bool>.Failure(
                            $"Choque de horario: el aula {dto.Classroom} ya está ocupada el " +
                            $"{FileRowParser.NombreDiaEnEspanol(dia.DiaSemana)} en ese rango.");
                }

                grupo.GroupCode = codigoGrupo;
                grupo.groupName = dto.GroupName.Trim();
                grupo.classroom = dto.Classroom.Trim();
                grupo.professorID = dto.ProfessorId;

                _context.RemoveRange(grupo.scheduleDaysFK);
                foreach (var dia in dto.Dias)
                {
                    _context.Add(new GroupSchedule
                    {
                        idGroupSchedule = Guid.NewGuid(),
                        idGroup = grupo.idGroup,
                        dayOfWeek = dia.DiaSemana,
                        startTime = dia.HoraInicio,
                        endTime = dia.HoraFin
                    });
                }

                await _context.SaveChangesAsync();
                return Response<bool>.Success(true, "Curso actualizado correctamente");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudo actualizar el curso");
            }
        }

        public async Task<Response<bool>> DeshabilitarCursoAsync(Guid idGroup) => await CambiarEstadoAsync(idGroup, false);

        public async Task<Response<bool>> HabilitarCursoAsync(Guid idGroup) => await CambiarEstadoAsync(idGroup, true);

        public async Task<Response<bool>> EliminarCursoAsync(Guid idGroup)
        {
            try
            {
                var grupo = await _context.Set<Group>()
                    .Include(g => g.classSessionsFK)
                    .Include(g => g.scheduleDaysFK)
                    .FirstOrDefaultAsync(g => g.idGroup == idGroup);

                if (grupo is null)
                    return Response<bool>.Failure("El curso no existe");

                if (grupo.classSessionsFK.Count > 0)
                    return Response<bool>.Failure(
                        $"No se puede eliminar: tiene {grupo.classSessionsFK.Count} sesión(es) de clase registrada(s). Desactívalo en su lugar.");

                _context.RemoveRange(grupo.scheduleDaysFK);
                _context.Remove(grupo);
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, "Curso eliminado correctamente");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudo eliminar el curso");
            }
        }

        public async Task<Response<List<ProfessorOptionDTO>>> ObtenerProfesoresDisponiblesAsync()
        {
            try
            {
                var profesores = await _context.Set<Professor>()
                    .Include(p => p.user)
                    .Select(p => new ProfessorOptionDTO
                    {
                        IdTeacher = p.idTeacher,
                        NombreCompleto = p.user.firstname + " " + p.user.lastName,
                        Email = p.user.email
                    })
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return Response<List<ProfessorOptionDTO>>.Success(profesores);
            }
            catch (Exception ex)
            {
                return Response<List<ProfessorOptionDTO>>.Failure(ex, "No se pudieron obtener los docentes");
            }
        }

        // ---------------- Helpers privados ----------------

        private async Task<Response<bool>> CambiarEstadoAsync(Guid idGroup, bool activar)
        {
            try
            {
                var grupo = await _context.Set<Group>().FindAsync(idGroup);
                if (grupo is null)
                    return Response<bool>.Failure("El curso no existe");

                grupo.isActive = activar;
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, activar ? "Curso habilitado" : "Curso deshabilitado");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudo cambiar el estado del curso");
            }
        }

        private static string CalcularSemestreActual()
        {
            var ahora = DateTime.UtcNow;
            var periodo = ahora.Month <= 6 ? 1 : 2;
            return $"{ahora.Year}-{periodo}";
        }

        private static CourseSummaryDTO MapearResumen(Group g)
        {
            var dias = g.scheduleDaysFK
                .OrderBy(d => d.dayOfWeek)
                .Select(d => new ScheduleDayDTO { DiaSemana = d.dayOfWeek, HoraInicio = d.startTime, HoraFin = d.endTime })
                .ToList();

            var resumenHorario = string.Join(" · ", dias.Select(d =>
                $"{FileRowParser.NombreDiaEnEspanol(d.DiaSemana)} {d.HoraInicio:hh\\:mm} - {d.HoraFin:hh\\:mm}"));

            return new CourseSummaryDTO
            {
                IdGroup = g.idGroup,
                SubjectCode = g.subjectFK?.subjectCode ?? string.Empty,
                SubjectName = g.subjectFK?.name ?? string.Empty,
                GroupCode = g.GroupCode,
                GroupName = g.groupName,
                Classroom = g.classroom,
                Semester = g.semester,
                ProfessorName = g.professorFK?.user is not null
                    ? $"{g.professorFK.user.firstname} {g.professorFK.user.lastName}"
                    : string.Empty,
                IsActive = g.isActive,
                TotalSesiones = g.classSessionsFK.Count,
                Dias = dias,
                HorarioResumen = resumenHorario
            };
        }
    }
}