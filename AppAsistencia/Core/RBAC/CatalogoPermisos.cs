namespace AppAsistencia.Core.RBAC
{
    public class SeccionPermisoInfo
    {
        public string Slug { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Banderas que definen qué acciones tiene permitidas la vista funcionalmente
        public bool SoportaVer { get; set; } = true;
        public bool SoportaCrear { get; set; } = true;
        public bool SoportaEditar { get; set; } = true;
        public bool SoportaEliminar { get; set; } = true;
    }

    public static class CatalogoPermisos
    {
        public static readonly List<SeccionPermisoInfo> Secciones = new()
        {
            new() {
                Slug = "registro-asistencia",
                RouteName = "/AttendanceRecord/CreateAssRecord",
                Nombre = "Registro de Asistencia",
                Descripcion = "Acceso a los métodos de registro (NFC, BLE, biométrico)",
                SoportaVer = true, SoportaCrear = true, SoportaEditar = false, SoportaEliminar = false // Solo Ver y Crear
            },
            new() {
                Slug = "registro-manual",
                RouteName = "/AttendanceRecord/CreateManualRecord",
                Nombre = "Registro Manual de Asistencia",
                Descripcion = "Registro de asistencia de forma manual con selección de clase",
                SoportaVer = true, SoportaCrear = true, SoportaEditar = false, SoportaEliminar = false // Solo Ver y Crear
            },
            new() {
                Slug = "confirmar-asistencias",
                RouteName = "/AttendanceRecord/attendanceConfirm",
                Nombre = "Confirmación de Asistencias",
                Descripcion = "Revisión y aprobación de asistencias manuales pendientes",
                SoportaVer = true, SoportaCrear = false, SoportaEditar = true, SoportaEliminar = false // Ver y Editar
            },
            new() {
                Slug = "ver-faltas",
                RouteName = "/AttendanceRecord/ViewFaults",
                Nombre = "Ver Faltas",
                Descripcion = "Consulta del historial de faltas y porcentaje de asistencia",
                SoportaVer = true, SoportaCrear = false, SoportaEditar = true, SoportaEliminar = true // lectura, edicion y eliminación
            },
            new() {
                Slug = "historial-clases",
                RouteName = "/ClassSession/ClassRecord",
                Nombre = "Historial de Clases",
                Descripcion = "Acceso al historial completo de clases del período académico",
                SoportaVer = true, SoportaCrear = false, SoportaEditar = true, SoportaEliminar = true // solo lectura (ver)
            },
            new() {
                Slug = "registrar-clase",
                RouteName = "/ClassSession/CreateClass",
                Nombre = "Registrar Clase",
                Descripcion = "Creación y configuración de nuevas sesiones de clase",
                SoportaVer = true, SoportaCrear = true, SoportaEditar = true, SoportaEliminar = true // Todas las operaciones
            },
            new() {
                Slug = "reportes",
                RouteName = "/Report/Index",
                Nombre = "Reportes",
                Descripcion = "Generación y exportación de informes de asistencia",
                SoportaVer = true, SoportaCrear = false, SoportaEditar = false, SoportaEliminar = false // Solo lectura (Ver)
            },
            new() {
                Slug = "notificaciones",
                RouteName = "/Notifications/Index",
                Nombre = "Notificaciones",
                Descripcion = "Gestión de alertas y avisos del sistema",
                SoportaVer = true, SoportaCrear = false, SoportaEditar = true, SoportaEliminar = true
            },
            new() {
                Slug = "configuracion",
                RouteName = "/Account/ConfigurationPage",
                Nombre = "Configuración",
                Descripcion = "Personalización de perfil, métodos de registro y privacidad",
                SoportaVer = true, SoportaCrear = false, SoportaEditar = true, SoportaEliminar = false
            },
            new() {
                Slug = "home",
                RouteName = "/Home/Index",
                Nombre = "Página de Inicio",
                Descripcion = "Acceso a la página principal del sistema",
                SoportaVer = true, SoportaCrear = false, SoportaEditar = false, SoportaEliminar = false
            },
        };

        public static string RoleNameDesdeSlug(string rolSlug) =>
            rolSlug == "docente" ? "Professor" : "Student";
    }
}