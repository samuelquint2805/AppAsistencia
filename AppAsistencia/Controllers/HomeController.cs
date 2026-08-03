using AppAsistencia.Core.RBAC;
using AppAsistencia.Models;

using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AppAsistencia.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [RequireRoutePermission("/Home/Index", PermissionType.View)]
        public IActionResult Index()
        {
            var model = new HomeViewModel
            {
                AccionesPrincipales = new List<AccionPrincipal>
                {
                    new()
                    {
                        Titulo = "Registrar Asistencia",
                        Descripcion = "Marca tu asistencia mediante NFC, BLE o Manual",
                        Icono = "bi-person-check",
                        Clases = "bg-blue-500 hover:bg-blue-600",
                        Href = "/attendanceRecord/CreateAssRecord"
                    },
                    new()
                    {
                        Titulo = "Historial de Clases",
                        Descripcion = "Consulta las clases a las que has asistido",
                        Icono = "bi-clipboard-check",
                        Clases = "bg-blue-600 hover:bg-blue-700",
                        Href = "/ClassSession/ClassRecord"
                    },
                    new()
                    {
                        Titulo = "Registrar Clase",
                        Descripcion = "Programar y gestionar sesiones de clase",
                        Icono = "bi-calendar-event",
                        Clases = "bg-blue-700 hover:bg-blue-800",
                        Href = "/ClassSession/CreateClass"
                    }
                },

                AccionesSecundarias = new List<AccionSecundaria>
                {
                    new() { Titulo = "Reportes", Descripcion = "Estadísticas y análisis", Icono = "bi-bar-chart-line", Href = "/Report/Index" },
                    new() { Titulo = "Notificaciones", Descripcion = "Alertas y avisos", Icono = "bi-bell", Href = "/Notifications/index" },
                    new() { Titulo = "Configuración", Descripcion = "Ajustes del sistema", Icono = "bi-gear", Href = "/Account/ConfigurationPage" },
                    new() { Titulo = "Ver Faltas", Descripcion = "Resumen de asistencia", Icono = "bi-clipboard-check", Href = "/AttendanceRecord/ViewFaults" },
                    new() { Titulo = "Confirmar Asistencias", Descripcion = "Aprobar registros manuales (Docentes)", Icono = "bi-person-check", Href = "/AttendanceRecord/attendanceConfirm" }
                },

                // TODO: reemplazar estos valores fijos por datos reales desde tu API/servicio
                Resumen = new ResumenHoyViewModel
                {
                    ClasesProgramadas = 4,
                    AsistenciasRegistradas = 3,
                    PorcentajeAsistencia = 85
                },

                ProximasClases = new List<ClaseProxima>
                {
                    new() { NombreClase = "Programación I", Horario = "10:00 AM", Aula = "Aula 301", ColorFondo = "bg-primary" },
                    new() { NombreClase = "Base de Datos", Horario = "2:00 PM", Aula = "Aula 205", ColorFondo = "bg-secondary" }
                }
            };

            return View(model);
        }
        public IActionResult ContactPage()
        {
            return View();
        }
        public IActionResult FeaturesPage()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
