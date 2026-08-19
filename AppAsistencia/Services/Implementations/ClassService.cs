using AppAsistencia.Core;
using AppAsistencia.Data.DBSET;
using AppAsistencia.DTOs;
using AppAsistencia.Models;
using AppAsistencia.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using static AppAsistencia.DTOs.Classoptionsdto;

namespace AppAsistencia.Services.Implementations
{
    // Requiere el paquete NuGet "ClosedXML" para leer archivos .xlsx:
    //   Install-Package ClosedXML
    public class ClassService : IClassService
    {
        private readonly DataContextAsistencia _context;
        private readonly IUserService _usuarioService;
        private readonly IEmailSenderService _emailSender;

        public ClassService(DataContextAsistencia context, IUserService usuarioService, IEmailSenderService emailSender)
        {
            _context = context;
            _usuarioService = usuarioService;
            _emailSender = emailSender;
        }

        public async Task<Response<GroupSummaryDTO>> RegistrarClaseAsync(RegisterClassDTO dto, Guid idProfessor)
        {
            try
            {
                var grupo = await _context.Set<Group>()
                    .Include(g => g.subjectFK)
                    .Include(g => g.scheduleDaysFK)
                    .Include(g => g.studentGroupsFK)
                    .FirstOrDefaultAsync(g => g.idGroup == dto.IdGroup);

                if (grupo is null)
                    return Response<GroupSummaryDTO>.Failure("El curso seleccionado no existe");

                if (!grupo.isActive)
                    return Response<GroupSummaryDTO>.Failure("Este curso está desactivado");

                // Seguridad: un docente solo puede agendar sesiones de SUS propios cursos asignados
                if (grupo.professorID != idProfessor)
                    return Response<GroupSummaryDTO>.Failure("No tienes permiso para agendar sesiones de este curso");

                // Busca si ese día de la semana está habilitado en el horario del curso
                var diaSemana = dto.SessionDate.DayOfWeek;
                var horarioDelDia = grupo.scheduleDaysFK.FirstOrDefault(gs => gs.dayOfWeek == diaSemana);

                if (horarioDelDia is null)
                {
                    var diasHabilitados = string.Join(", ", grupo.scheduleDaysFK
                        .Select(gs => FileRowParser.NombreDiaEnEspanol(gs.dayOfWeek)));

                    return Response<GroupSummaryDTO>.Failure(
                        $"Este curso no tiene clase programada el {FileRowParser.NombreDiaEnEspanol(diaSemana)}. " +
                        $"Días habilitados: {diasHabilitados}.");
                }

                var nuevoInicio = dto.SessionDate.Date.Add(horarioDelDia.startTime);
                var nuevoFin = dto.SessionDate.Date.Add(horarioDelDia.endTime);

                // Evita registrar dos veces la sesión del mismo curso en la misma fecha
                var yaExisteEsaFecha = await _context.Set<ClassSession>()
                    .AnyAsync(cs => cs.groupID == dto.IdGroup && cs.startTime.Date == dto.SessionDate.Date);

                if (yaExisteEsaFecha)
                    return Response<GroupSummaryDTO>.Failure("Ya existe una sesión registrada para este curso en esa fecha");

                // Salvaguarda: choque de aula (por si dos cursos distintos quedaron mal configurados)
                var conflicto = await _context.Set<ClassSession>()
                    .Include(cs => cs.groupFK)
                    .AnyAsync(cs => cs.groupID != dto.IdGroup
                                 && cs.groupFK.classroom == grupo.classroom
                                 && cs.startTime < nuevoFin
                                 && cs.endTime > nuevoInicio);

                if (conflicto)
                    return Response<GroupSummaryDTO>.Failure(
                        "No es posible registrar esta sesión: ya hay otra clase en esa aula y horario. " +
                        "Si crees que esto es un error, comunícate con sistemas.");

                var sesion = new ClassSession
                {
                    idSession = Guid.NewGuid(),
                    startTime = nuevoInicio,
                    endTime = nuevoFin,
                    status = "Programada",
                    groupID = grupo.idGroup
                };

                _context.Add(sesion);
                await _context.SaveChangesAsync();

                return Response<GroupSummaryDTO>.Success(new GroupSummaryDTO
                {
                    IdGroup = grupo.idGroup,
                    SubjectName = grupo.subjectFK?.name ?? string.Empty,
                    GroupName = grupo.groupName,
                    Classroom = grupo.classroom,
                    Semester = grupo.semester,
                    IsActive = grupo.isActive,
                    TotalEstudiantes = grupo.studentGroupsFK.Count,
                    ProximaSesion = nuevoInicio
                }, "Sesión de clase registrada correctamente");
            }
            catch (Exception ex)
            {
                return Response<GroupSummaryDTO>.Failure(ex, "No se pudo registrar la sesión de clase");
            }
        }

        public async Task<Response<bool>> RegistrarEstudiantesAsync(AddStudentsToGroupDTO dto)
        {
            try
            {
                var grupo = await _context.Set<Group>().FindAsync(dto.IdGroup);
                if (grupo is null)
                    return Response<bool>.Failure("El grupo no existe");

                foreach (var idStudent in dto.StudentIds.Distinct())
                {
                    var yaInscrito = await _context.Set<StudentGroup>()
                        .AnyAsync(sg => sg.studentID == idStudent && sg.GroupID == dto.IdGroup);

                    if (!yaInscrito)
                    {
                        _context.Add(new StudentGroup
                        {
                            studentID = idStudent,
                            GroupID = dto.IdGroup,
                            enrollmentDate = DateTime.UtcNow.ToString("O")
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Response<bool>.Success(true, "Estudiantes inscritos correctamente");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudieron inscribir los estudiantes");
            }
        }

        public async Task<Response<ImportStudentsResultDTO>> RegistrarEstudiantesDesdeArchivoAsync(Guid idGroup, Stream archivo, string nombreArchivo)
        {
            var resultado = new ImportStudentsResultDTO();

            try
            {
                var grupo = await _context.Set<Group>().FindAsync(idGroup);
                if (grupo is null)
                    return Response<ImportStudentsResultDTO>.Failure("El grupo no existe");

                var filas = ExtraerFilas(archivo, nombreArchivo, resultado);

                foreach (var fila in filas)
                {
                    try
                    {
                        var email = fila.GetValueOrDefault("email")?.Trim();
                        var carnet = fila.GetValueOrDefault("studentIdCard")?.Trim();
                        var nombre = fila.GetValueOrDefault("firstName")?.Trim();
                        var apellido = fila.GetValueOrDefault("lastName")?.Trim();

                        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(carnet))
                        {
                            resultado.FilasConError++;
                            resultado.Errores.Add($"Fila con correo '{email}': faltan datos obligatorios (correo o carnet)");
                            continue;
                        }

                        var estudianteExistente = await _usuarioService.ObtenerPorEmailAsync(email);
                        Guid idStudent;

                        if (estudianteExistente.IsSuccess && estudianteExistente.Result is not null)
                        {
                            idStudent = estudianteExistente.Result.idUser;
                            resultado.EstudiantesVinculados++;
                        }
                        else
                        {
                            var passwordTemporal = Guid.NewGuid().ToString("N")[..12];

                            var registroDto = new RegisterDTO
                            {
                                
                                UserName = email.Split('@')[0],
                                FirstName = nombre ?? "Estudiante",
                                LastName = apellido ?? string.Empty,
                                Email = email,
                                Password = passwordTemporal,
                                Role = RoleType.Student,
                                studentIdCard = carnet,
                                currentSemester = 1,
                                phoneNumber = fila.GetValueOrDefault("phoneNumber")
                            };

                            var creado = await _usuarioService.RegistrarUsuarioAsync(registroDto);
                            if (!creado.IsSuccess || creado.Result is null)
                            {
                                resultado.FilasConError++;
                                resultado.Errores.Add($"No se pudo crear al estudiante '{email}': {creado.Message}");
                                continue;
                            }

                            idStudent = creado.Result.IdUser;
                            resultado.EstudiantesCreados++;

                            try
                            {
                                await _emailSender.SendEmailAsync(
                                    email,
                                    "Acceso a AppAsistencia",
                                    $"<p>Hola {nombre}, tu docente te registró en el sistema de asistencia ITM.</p>" +
                                    $"<p>Tu contraseña temporal es: <b>{passwordTemporal}</b></p>" +
                                    $"<p>Por seguridad, cámbiala apenas ingreses por primera vez desde Configuración.</p>");
                            }
                            catch
                            {
                                // El estudiante ya quedó creado; el correo se puede reenviar despues manualmente.
                            }
                        }

                        var yaInscrito = await _context.Set<StudentGroup>()
                            .AnyAsync(sg => sg.studentID == idStudent && sg.GroupID == idGroup);

                        if (!yaInscrito)
                        {
                            _context.Add(new StudentGroup
                            {
                                studentID = idStudent,
                                GroupID = idGroup,
                                enrollmentDate = DateTime.UtcNow.ToString("O")
                            });
                        }
                    }
                    catch (Exception exFila)
                    {
                        resultado.FilasConError++;
                        resultado.Errores.Add($"Error inesperado procesando una fila: {exFila.Message}");
                    }
                }

                await _context.SaveChangesAsync();
                return Response<ImportStudentsResultDTO>.Success(resultado, "Archivo procesado");
            }
            catch (Exception ex)
            {
                return Response<ImportStudentsResultDTO>.Failure(ex, "No se pudo procesar el archivo");
            }
        }

        public async Task<Response<List<GroupSummaryDTO>>> ObtenerClasesDelDocenteAsync(Guid idProfessor)
        {
            try
            {
                var grupos = await _context.Set<Group>()
                    .Include(g => g.subjectFK)
                    .Include(g => g.studentGroupsFK)
                    .Include(g => g.classSessionsFK)
                    .Where(g => g.professorID == idProfessor)
                    .ToListAsync();

                var resultado = grupos.Select(g => new GroupSummaryDTO
                {
                    IdGroup = g.idGroup,
                    SubjectName = g.subjectFK?.name ?? string.Empty,
                    GroupName = g.groupName,
                    Classroom = g.classroom,
                   
                    Semester = g.semester,
                    IsActive = g.isActive,
                    TotalEstudiantes = g.studentGroupsFK.Count,
                    ProximaSesion = g.classSessionsFK
                        .Where(cs => cs.startTime >= DateTime.UtcNow)
                        .OrderBy(cs => cs.startTime)
                        .Select(cs => (DateTime?)cs.startTime)
                        .FirstOrDefault()
                }).ToList();

                return Response<List<GroupSummaryDTO>>.Success(resultado);
            }
            catch (Exception ex)
            {
                return Response<List<GroupSummaryDTO>>.Failure(ex, "No se pudieron obtener las clases");
            }
        }

        public async Task<Response<GroupSummaryDTO>> ObtenerClasePorIdAsync(Guid idGroup)
        {
            try
            {
                var g = await _context.Set<Group>()
                    .Include(x => x.subjectFK)
                    .Include(x => x.studentGroupsFK)
                    .Include(x => x.classSessionsFK)
                    .FirstOrDefaultAsync(x => x.idGroup == idGroup);

                if (g is null)
                    return Response<GroupSummaryDTO>.Failure("La clase no existe");

                return Response<GroupSummaryDTO>.Success(new GroupSummaryDTO
                {
                    IdGroup = g.idGroup,
                    SubjectName = g.subjectFK?.name ?? string.Empty,
                    GroupName = g.groupName,
                    Classroom = g.classroom,
                    Semester = g.semester,
                    IsActive = g.isActive,
                    TotalEstudiantes = g.studentGroupsFK.Count,
                    ProximaSesion = g.classSessionsFK
                        .Where(cs => cs.startTime >= DateTime.UtcNow)
                        .OrderBy(cs => cs.startTime)
                        .Select(cs => (DateTime?)cs.startTime)
                        .FirstOrDefault()
                });
            }
            catch (Exception ex)
            {
                return Response<GroupSummaryDTO>.Failure(ex, "No se pudo obtener la clase");
            }
        }

        public async Task<Response<bool>> EditarClaseAsync(EditClassDTO dto)
        {
            try
            {
                var grupo = await _context.Set<Group>().FindAsync(dto.IdGroup);
                if (grupo is null)
                    return Response<bool>.Failure("La clase no existe");

                // Si cambia el aula, revalida choques contra las sesiones futuras de este grupo
                if (!string.Equals(grupo.classroom, dto.Classroom, StringComparison.OrdinalIgnoreCase))
                {
                    var sesionesFuturas = await _context.Set<ClassSession>()
                        .Where(cs => cs.groupID == dto.IdGroup && cs.startTime >= DateTime.UtcNow)
                        .ToListAsync();

                    foreach (var sesion in sesionesFuturas)
                    {
                        var conflicto = await _context.Set<ClassSession>()
                            .Include(cs => cs.groupFK)
                            .AnyAsync(cs => cs.idSession != sesion.idSession
                                         && cs.groupFK.classroom == dto.Classroom
                                         && cs.startTime < sesion.endTime
                                         && cs.endTime > sesion.startTime);

                        if (conflicto)
                            return Response<bool>.Failure(
                                "No es posible mover esta clase a esa aula: ya hay una sesión programada en ese horario. " +
                                "Si crees que esto es un error, comunícate con sistemas.");
                    }
                }

                grupo.groupName = dto.GroupName;
                grupo.classroom = dto.Classroom;

                await _context.SaveChangesAsync();
                return Response<bool>.Success(true, "Clase actualizada correctamente");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudo actualizar la clase");
            }
        }

        public async Task<Response<bool>> DesactivarClaseAsync(Guid idGroup)
        {
            try
            {
                var grupo = await _context.Set<Group>().FindAsync(idGroup);
                if (grupo is null)
                    return Response<bool>.Failure("La clase no existe");

                grupo.isActive = false;
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, "Clase desactivada correctamente");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "No se pudo desactivar la clase");
            }
        }

        public async Task<Response<List<StudentOptionDTO>>> ObtenerTodosLosEstudiantesAsync()
        {
            try
            {
                var estudiantes = await _context.Set<Student>()
                    .Include(s => s.user)
                    .Select(s => new StudentOptionDTO
                    {
                        IdStudent = s.idStudent,
                        NombreCompleto = s.user.firstname + " " + s.user.lastName,
                        StudentIdCard = s.studentIdCard,
                        Email = s.user.email
                    })
                    .OrderBy(s => s.NombreCompleto)
                    .ToListAsync();

                return Response<List<StudentOptionDTO>>.Success(estudiantes);
            }
            catch (Exception ex)
            {
                return Response<List<StudentOptionDTO>>.Failure(ex, "No se pudieron obtener los estudiantes");
            }
        }

       

        // ---------------- Helpers privados ----------------

      

        private List<Dictionary<string, string>> ExtraerFilas(Stream archivo, string nombreArchivo, ImportStudentsResultDTO resultado)
        {
            var filas = new List<Dictionary<string, string>>();
            var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();

            if (extension == ".csv")
            {
                using var lector = new StreamReader(archivo);
                var encabezado = lector.ReadLine()?.Split(',').Select(h => h.Trim()).ToArray();
                if (encabezado is null) return filas;

                string? linea;
                while ((linea = lector.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    var valores = linea.Split(',');
                    var fila = new Dictionary<string, string>();
                    for (int i = 0; i < encabezado.Length && i < valores.Length; i++)
                        fila[encabezado[i]] = valores[i].Trim();
                    filas.Add(fila);
                }
            }
            else if (extension == ".xlsx")
            {
                using var libro = new ClosedXML.Excel.XLWorkbook(archivo);
                var hoja = libro.Worksheet(1);
                var encabezados = hoja.Row(1).CellsUsed().Select(c => c.GetString().Trim()).ToList();

                foreach (var filaExcel in hoja.RowsUsed().Skip(1))
                {
                    var fila = new Dictionary<string, string>();
                    for (int i = 0; i < encabezados.Count; i++)
                        fila[encabezados[i]] = filaExcel.Cell(i + 1).GetString().Trim();
                    filas.Add(fila);
                }
            }
            else
            {
                resultado.Errores.Add("Formato de archivo no soportado. Usa .csv o .xlsx");
            }

            return filas;
        }
    }
}
