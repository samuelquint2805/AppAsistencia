using AppAsistencia.Core;
using AppAsistencia.Data.DBSET;
using AppAsistencia.DTOs;
using AppAsistencia.Models;
using AppAsistencia.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace AppAsistencia.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly DataContextAsistencia _context;
        private readonly IEmailSenderService _emailSender;
        //private const string DominioInstitucional1 = "@correo.itm.edu.co";
        private const string DominioInstitucional1 = "";
        private const string DominioInstitucional2 = "";
        //private const string DominioInstitucional2 = "@itm.edu.co";

        public UserService(DataContextAsistencia context, IEmailSenderService emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<Response<UserSummaryDTO>> RegistrarUsuarioAsync(RegisterDTO dto)
        {
            try
            {
                // 1. Regla de negocio: solo correos institucionales
                if (!EsCorreoInstitucionalValido(dto.Email))
                    return Response<UserSummaryDTO>.Failure(
                        $"Solo se permiten correos institucionales terminados en {DominioInstitucional1} o {DominioInstitucional2}");

                // 2. Correo único
                var yaExiste = await _context.Set<User>().AnyAsync(u => u.email == dto.Email);
                if (yaExiste)
                    return Response<UserSummaryDTO>.Failure("Ya existe un usuario registrado con este correo");

                // 3. Resolver el rol solicitado contra ROLEMANAGEMENT
                //    Ajusta "r.nombreRol" al nombre real de la propiedad en tu clase Role si difiere.
                var roleName = dto.Role.ToString();
                var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.nombreRol == roleName);
                if (role is null)
                    return Response<UserSummaryDTO>.Failure($"El rol '{roleName}' no está configurado en el sistema");

                // 4. Validaciones específicas por rol antes de abrir la transacción
                if (dto.Role == RoleType.Student && (string.IsNullOrWhiteSpace(dto.studentIdCard) || dto.currentSemester is null))
                    return Response<UserSummaryDTO>.Failure("carnet institucional y semestre actual son requeridos");

                if (dto.Role == RoleType.Professor && (string.IsNullOrWhiteSpace(dto.professorIdCard) || string.IsNullOrWhiteSpace(dto.department)))
                    return Response<UserSummaryDTO>.Failure("carnet de profesor y departamento son requeridos");

                // 5. Transacción: el id del usuario se reutiliza como PK/FK compartida
                //    en la tabla específica del rol.
                await using var transaction = await _context.Database.BeginTransactionAsync();

                var idUser = Guid.NewGuid();

                var user = new User
                {
                    idUser = idUser,
                    userName = dto.UserName,
                    firstname = dto.FirstName,
                    lastName = dto.LastName,
                    email = dto.Email,
                    passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    isActive = true,
                    isEmailConfirmed = false,
                    registerDate = DateTime.UtcNow,
                    accountRenewalDate = DateTime.UtcNow.AddMonths(6),
                    idRol = role.idRol
                };
                _context.Add(user);

                switch (dto.Role)
                {
                    case RoleType.Student:
                        _context.Add(new Student
                        {
                            idStudent = idUser,
                            studentIdCard = dto.studentIdCard!,
                            currentSemester = dto.currentSemester!.Value,
                            phoneNumber = dto.phoneNumber ?? string.Empty
                        });
                        break;

                    case RoleType.Professor:
                        _context.Add(new Professor
                        {
                            idTeacher = idUser,
                            professorIdCard = dto.professorIdCard!,
                            phoneNumber = dto.phoneNumber ?? string.Empty,
                            department = dto.department!

                        });
                        break;

                    case RoleType.Administrator:
                        _context.Add(new Administrator
                        {
                            idAdmin = idUser,
                            phoneNumber = dto.phoneNumber ?? string.Empty
                        });
                        break;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                
                var resultado = new UserSummaryDTO
                {
                    IdUser = user.idUser,
                    UserName = user.userName,
                    Email = user.email,
                    Role = roleName,
                    IsEmailConfirmed = user.isEmailConfirmed
                };

                return Response<UserSummaryDTO>.Success(resultado, "Usuario registrado con éxito");
            }
            catch (Exception ex)
            {
                return Response<UserSummaryDTO>.Failure(ex, "Ocurrió un error al registrar el usuario");
            }
        }

        public async Task<Response<bool>> ConfirmarEmailDirectoAsync(Guid idUser)
        {
            try
            {
                var user = await _context.Set<User>().FindAsync(idUser);
                if (user is null)
                    return Response<bool>.Failure("Usuario no encontrado");

                user.isEmailConfirmed = true;
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, "Correo confirmado");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al confirmar el correo");
            }
        }
        public async Task<Response<User>> ValidarCredencialesAsync(string email, string password)
        {
            try
            {
                var userResponse = await ObtenerPorEmailAsync(email);
                if (!userResponse.IsSuccess || userResponse.Result is null)
                    return Response<User>.Failure("Credenciales inválidas");

                var user = userResponse.Result;

                if (!user.isActive)
                    return Response<User>.Failure("El usuario se encuentra inactivo");

                var esValida = BCrypt.Net.BCrypt.Verify(password, user.passwordHash);
                if (!esValida)
                    return Response<User>.Failure("Credenciales inválidas");

                return Response<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Response<User>.Failure(ex, "Ocurrió un error al validar las credenciales");
            }
        }

        public async Task<Response<User>> ObtenerPorEmailAsync(string email)
        {
            try
            {
                var user = await _context.Set<User>()
                    .Include(u => u.roleFK)
                    .FirstOrDefaultAsync(u => u.email == email);

                return user is null
                    ? Response<User>.Failure("Usuario no encontrado")
                    : Response<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Response<User>.Failure(ex, "Ocurrió un error al buscar el usuario");
            }
        }

        public async Task<Response<User>> ObtenerPorIdAsync(Guid idUser)
        {
            try
            {
                var user = await _context.Set<User>()
                    .Include(u => u.roleFK)
                    .FirstOrDefaultAsync(u => u.idUser == idUser);

                return user is null
                    ? Response<User>.Failure("Usuario no encontrado")
                    : Response<User>.Success(user);
            }
            catch (Exception ex)
            {
                return Response<User>.Failure(ex, "Ocurrió un error al buscar el usuario");
            }
        }

        public async Task<Response<bool>> CambiarEstadoUsuarioAsync(Guid idUser, bool activar)
        {
            try
            {
                var user = await _context.Set<User>().FindAsync(idUser);
                if (user is null)
                    return Response<bool>.Failure("Usuario no encontrado");

                user.isActive = activar;
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, activar ? "Usuario activado" : "Usuario desactivado");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al cambiar el estado del usuario");
            }
        }

        public Task<Response<bool>> ValidarEmailAsync(string email)
        {
            var esValido = EsCorreoInstitucionalValido(email);
            var respuesta = esValido
                ? Response<bool>.Success(true, "Correo institucional válido")
                : Response<bool>.Failure($"Solo se permiten correos institucionales terminados en {DominioInstitucional1} o {DominioInstitucional2}");

            return Task.FromResult(respuesta);
        }

        public Task<Response<string>> GenerarTokenConfirmacionAsync(Guid idUser)
        {
            // Token sin estado: idUser + expiración codificados en base64.
            // Para producción, considera firmarlo con HMAC usando una clave de configuración.
            var payload = $"{idUser}|{DateTime.UtcNow.AddHours(24):O}";
            var token = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
            return Task.FromResult(Response<string>.Success(token));
        }

        public async Task<Response<bool>> ConfirmarEmailAsync(Guid idUser, string token)
        {
            try
            {
                string payload;
                try
                {
                    payload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
                }
                catch
                {
                    return Response<bool>.Failure("Token de confirmación inválido");
                }

                var partes = payload.Split('|');
                if (partes.Length != 2 || partes[0] != idUser.ToString())
                    return Response<bool>.Failure("Token de confirmación inválido");

                if (!DateTime.TryParse(partes[1], null, System.Globalization.DateTimeStyles.RoundtripKind, out var expiracion)
                    || expiracion < DateTime.UtcNow)
                    return Response<bool>.Failure("El token de confirmación ha expirado");

                var user = await _context.Set<User>().FindAsync(idUser);
                if (user is null)
                    return Response<bool>.Failure("Usuario no encontrado");

                user.isEmailConfirmed = true;
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, "Correo confirmado con éxito");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al confirmar el correo");
            }
        }

        private static bool EsCorreoInstitucionalValido(string email)
        {
            return !string.IsNullOrWhiteSpace(email)
                && (email.Trim().EndsWith(DominioInstitucional1, StringComparison.OrdinalIgnoreCase) ||
                     email.Trim().EndsWith(DominioInstitucional2, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Response<bool>> ActualizarPerfilAsync(Guid idUser, string firstName, string lastName, string email)
        {
            try
            {
                var user = await _context.Set<User>().FindAsync(idUser);
                if (user is null)
                    return Response<bool>.Failure("Usuario no encontrado");

                if (!EsCorreoInstitucionalValido(email))
                    return Response<bool>.Failure($"Solo se permiten correos institucionales terminados en {DominioInstitucional1} o {DominioInstitucional2}");

                // Si el correo cambió, verifica que el nuevo no esté en uso por otra cuenta
                if (!string.Equals(user.email, email, StringComparison.OrdinalIgnoreCase))
                {
                    var yaExiste = await _context.Set<User>().AnyAsync(u => u.email == email && u.idUser != idUser);
                    if (yaExiste)
                        return Response<bool>.Failure("Ese correo ya está en uso por otra cuenta");
                }

                user.firstname = firstName;
                user.lastName = lastName;
                user.email = email;

                await _context.SaveChangesAsync();
                return Response<bool>.Success(true, "Perfil actualizado");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al actualizar el perfil");
            }
        }

        public async Task<Response<bool>> CambiarContrasenaAsync(Guid idUser, string actual, string nueva)
        {
            try
            {
                var user = await _context.Set<User>().FindAsync(idUser);
                if (user is null)
                    return Response<bool>.Failure("Usuario no encontrado");

                if (!BCrypt.Net.BCrypt.Verify(actual, user.passwordHash))
                    return Response<bool>.Failure("La contraseña actual no es correcta");

                user.passwordHash = BCrypt.Net.BCrypt.HashPassword(nueva);
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, "Contraseña actualizada");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al cambiar la contraseña");
            }
        }

        public async Task<Response<Student>> ObtenerEstudiantePorIdAsync(Guid idUser)
        {
            try
            {
                var estudiante = await _context.Set<Student>().FindAsync(idUser);
                return estudiante is null
                    ? Response<Student>.Failure("No se encontró información de estudiante")
                    : Response<Student>.Success(estudiante);
            }
            catch (Exception ex)
            {
                return Response<Student>.Failure(ex, "Ocurrió un error al buscar el estudiante");
            }
        }
        public async Task<Response<Professor>> ObtenerProfesorPorIdAsync(Guid idUser)
        {
            try
            {
                var profesor = await _context.Set<Professor>().FindAsync(idUser);
                return profesor is null
                    ? Response<Professor>.Failure("No se encontró información de docente")
                    : Response<Professor>.Success(profesor);
            }
            catch (Exception ex)
            {
                return Response<Professor>.Failure(ex, "Ocurrió un error al buscar el docente");
            }
        }

        public async Task<Response<Administrator>> ObtenerAdministradorPorIdAsync(Guid idUser)
        {
            try
            {
                var admin = await _context.Set<Administrator>().FindAsync(idUser);
                return admin is null
                    ? Response<Administrator>.Failure("No se encontró información de administrador")
                    : Response<Administrator>.Success(admin);
            }
            catch (Exception ex)
            {
                return Response<Administrator>.Failure(ex, "Ocurrió un error al buscar el administrador");
            }
        }

        public async Task<Response<bool>> ActualizarSemestreAsync(Guid idUser, int nuevoSemestre)
        {
            try
            {
                var estudiante = await _context.Set<Student>().FindAsync(idUser);
                if (estudiante is null)
                    return Response<bool>.Failure("No se encontró información de estudiante");

                estudiante.currentSemester = nuevoSemestre;
                await _context.SaveChangesAsync();

                return Response<bool>.Success(true, "Semestre actualizado");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al actualizar el semestre");
            }
        }
        public async Task<Response<bool>> ActualizarDatosEstudianteAsync(Guid idUser, string? phoneNumber, int currentSemester, string? studentIdCard)
        {
            try
            {
                var estudiante = await _context.Set<Student>().FindAsync(idUser);
                if (estudiante is null)
                    return Response<bool>.Failure("No se encontró información de estudiante");

                estudiante.currentSemester = currentSemester;

                if (!string.IsNullOrWhiteSpace(studentIdCard))
                    estudiante.studentIdCard = studentIdCard;

                if (!string.IsNullOrWhiteSpace(phoneNumber))
                    estudiante.phoneNumber = phoneNumber;

                await _context.SaveChangesAsync();
                return Response<bool>.Success(true, "Datos de estudiante actualizados");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al actualizar los datos del estudiante");
            }
        }

        public async Task<Response<bool>> ActualizarDatosProfesorAsync(Guid idUser, string? phoneNumber, string? department, string? professorIdCard)
        {
            try
            {
                var profesor = await _context.Set<Professor>().FindAsync(idUser);
                if (profesor is null)
                    return Response<bool>.Failure("No se encontró información de docente");

                if (!string.IsNullOrWhiteSpace(department))
                    profesor.department = department;
                
                if (!string.IsNullOrWhiteSpace(professorIdCard))
                    profesor.professorIdCard = professorIdCard;

                if (!string.IsNullOrWhiteSpace(phoneNumber))
                    profesor.phoneNumber = phoneNumber;

                await _context.SaveChangesAsync();
                return Response<bool>.Success(true, "Datos de docente actualizados");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al actualizar los datos del docente");
            }
        }

        public async Task<Response<bool>> ActualizarDatosAdministradorAsync(Guid idUser, string? phoneNumber)
        {
            try
            {
                var admin = await _context.Set<Administrator>().FindAsync(idUser);
                if (admin is null)
                    return Response<bool>.Failure("No se encontró información de administrador");

                if (!string.IsNullOrWhiteSpace(phoneNumber))
                    admin.phoneNumber = phoneNumber;

                await _context.SaveChangesAsync();
                return Response<bool>.Success(true, "Datos de administrador actualizados");
            }
            catch (Exception ex)
            {
                return Response<bool>.Failure(ex, "Ocurrió un error al actualizar los datos del administrador");
            }
        }
    }
}
