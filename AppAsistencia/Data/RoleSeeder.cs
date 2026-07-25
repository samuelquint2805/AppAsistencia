using AppAsistencia.Data.DBSET;
using AppAsistencia.Models;
using Microsoft.EntityFrameworkCore;

namespace AppAsistencia.Data
{
    public class RoleRouteSeeder
    {
        
        public async Task SeedAsync(DataContextAsistencia context)
        {
            await SeedRolesAsync(context);
            await SeedRoutesAsync(context);
            await SeedRouteRolesAsync(context);
        }

        private async Task SeedRolesAsync(DataContextAsistencia context)
        {
            // IMPORTANTE: estos nombres deben coincidir EXACTAMENTE con los valores del
            // enum RoleType (Student, Professor, Administrator) en RegisterDTO.cs, porque
            // UsuarioService.RegistrarUsuarioAsync busca el rol por dto.Role.ToString().
            var rolesEsperados = new[]
            {
                new Role { idRol = Guid.NewGuid(), nombreRol = "Student", descripcion = "Estudiante del ITM" },
                new Role { idRol = Guid.NewGuid(), nombreRol = "Professor", descripcion = "Docente del ITM" },
                new Role { idRol = Guid.NewGuid(), nombreRol = "Administrator", descripcion = "Administrador del sistema" },
            };

            foreach (var rol in rolesEsperados)
            {
                var existe = await context.Set<Role>().AnyAsync(r => r.nombreRol == rol.nombreRol);
                if (!existe)
                {
                    context.Set<Role>().Add(rol);
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task SeedRoutesAsync(DataContextAsistencia context)
        {
            // Ajusta esta lista a las rutas reales de tu aplicacion.
            var rutasEsperadas = new[]
            {
                "/Home/Index",
                "/Account/ConfigurationPage",
                "/Account/ForgotPassword",
                "/Account/LoginExitoso",
                "/Account/RegisterPage",
                "/Account/Verification2FA",
                "/AttendanceRecord/attendanceConfirm",
                "/AttendanceRecord/CreateAssRecord",
                "/AttendanceRecord/CreateManualRecord",
                "/AttendanceRecord/ViewFaults",
                "/ClassSession/ClassRecord",
                "/ClassSession/CreateClass",
                "/Notifications/Index",
                "/Report/Index",
                "/Role/RolePermissions",
                "/Account/LoginSelection",
                "/Account/LoginDocente",
                "/Account/LoginEstudiante",
                "/Account/LoginSelection",
            };

            foreach (var nombreRuta in rutasEsperadas)
            {
                var existe = await context.Set<RoutesAs>().AnyAsync(r => r.routeName == nombreRuta);
                if (!existe)
                {
                    context.Set<RoutesAs>().Add(new RoutesAs
                    {
                        idRoute = Guid.NewGuid(),
                        routeName = nombreRuta
                    });
                }
            }

            await context.SaveChangesAsync();
        }

        private async Task SeedRouteRolesAsync(DataContextAsistencia context)
        {
            var roles = await context.Set<Role>().ToListAsync();
            var rutas = await context.Set<RoutesAs>().ToListAsync();

            var student = roles.FirstOrDefault(r => r.nombreRol == "Student");
            var professor = roles.FirstOrDefault(r => r.nombreRol == "Professor");
            var admin = roles.FirstOrDefault(r => r.nombreRol == "Administrator");

            if (student is null || professor is null || admin is null)
            {
                return; // Los roles deben existir primero (se siembran en SeedRolesAsync)
            }

            // Tabla de permisos: (ruta, rol, ver, crear, editar, eliminar)
            // Ajusta segun las reglas de negocio reales de cada HU.
            var permisos = new List<(string ruta, Guid rolId, bool ver, bool crear, bool editar, bool eliminar)>
            { 
            // Panel principal: todos los roles autenticados lo ven
                ("/Home/Index", student.idRol, true, false, false, false),
                ("/Home/Index", professor.idRol, true, false, false, false),
                ("/Home/Index", admin.idRol, true, false, false, false),

                // Configuracion del sistema: solo Admin
                ("/Account/ConfigurationPage", admin.idRol, true, true, true, true),

                // Confirmar asistencias manuales: Docente (HU8 - fallback de identidad)
                ("/AttendanceRecord/attendanceConfirm", professor.idRol, true, false, true, false),

                // Registrar asistencia (NFC/BLE/Manual): Estudiante
                ("/AttendanceRecord/CreateAssRecord", student.idRol, true, true, false, false),

                // Crear registro manual de asistencia: Docente
                ("/AttendanceRecord/CreateManualRecord", professor.idRol, true, true, false, false),

                // Ver faltas / resumen de asistencia: Docente y Admin
                ("/AttendanceRecord/ViewFaults", professor.idRol, true, false, false, false),
                ("/AttendanceRecord/ViewFaults", admin.idRol, true, false, false, false),

                // Historial de clases: Estudiante y Docente
                ("/ClassSession/ClassRecord", student.idRol, true, false, false, false),
                ("/ClassSession/ClassRecord", professor.idRol, true, false, false, false),

                // Registrar/programar clase: Docente
                ("/ClassSession/CreateClass", professor.idRol, true, true, true, false),

                // Notificaciones: todos ven; Admin administra
                ("/Notifications/Index", student.idRol, true, false, false, false),
                ("/Notifications/Index", professor.idRol, true, false, false, false),
                ("/Notifications/Index", admin.idRol, true, true, true, true),

                // Reportes: Docente y Admin
                ("/Report/Index", professor.idRol, true, false, false, false),
                ("/Report/Index", admin.idRol, true, false, false, false),

                // Gestion de roles y permisos: solo Admin
                ("/Role/RolePermissions", admin.idRol, true, true, true, true),
            };

            foreach (var (rutaNombre, rolId, ver, crear, editar, eliminar) in permisos)
            {
                var ruta = rutas.FirstOrDefault(r => r.routeName == rutaNombre);
                if (ruta is null) continue;

                var yaExiste = await context.Set<RouteRole>()
                    .AnyAsync(rr => rr.routeID == ruta.idRoute && rr.roleID == rolId);

                if (!yaExiste)
                {
                    context.Set<RouteRole>().Add(new RouteRole
                    {
                        routeID = ruta.idRoute,
                        roleID = rolId,
                        canView = ver,
                        canCreate = crear,
                        canEdit = editar,
                        canDelete = eliminar
                    });
                }
            }

            await context.SaveChangesAsync();
        }

    }
}
