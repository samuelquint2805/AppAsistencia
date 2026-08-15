using AppAsistencia.Core.RBAC;
using AppAsistencia.DTOs;
using AppAsistencia.Services.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static AppAsistencia.DTOs.Classoptionsdto;

namespace AppAsistencia.Controllers
{
    public class ClassSessionController : Controller
    {
        private readonly IClassService _classService;

        public ClassSessionController(IClassService classService)
        {
            _classService = classService;
        }

        // ---------- Paso 1: Registrar clase ----------

        [HttpGet]
        [RequireRoutePermission("/ClassSession/CreateClass", PermissionType.Create)]
        public async Task<IActionResult> CreateClass()
        {
            var cursos = await _classService.ObtenerCursosActivosAsync();
            ViewBag.Cursos = cursos.Result ?? new List<SubjectOptionDTO>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRoutePermission("/ClassSession/CreateClass", PermissionType.Create)]
        public async Task<IActionResult> CreateClass(RegisterClassDTO model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorClase"] = "Revisa los campos del formulario";
                return RedirectToAction(nameof(ClassRegistrationFailed));
            }

            var idProfessor = ObtenerIdUsuarioActual();
            if (idProfessor is null)
                return RedirectToAction("LoginSelection", "Account");

            var resultado = await _classService.RegistrarClaseAsync(model, idProfessor.Value);

            if (!resultado.IsSuccess || resultado.Result is null)
            {
                TempData["ErrorClase"] = resultado.Message;
                return RedirectToAction(nameof(ClassRegistrationFailed));
            }

            return RedirectToAction(nameof(RegisterStudents), new { idGroup = resultado.Result.IdGroup });
        }

        // ---------- Paso 2: Registrar estudiantes (dropdown + archivo) ----------

        [HttpGet]
        [RequireRoutePermission("/ClassSession/CreateClass", PermissionType.Create)]
        public async Task<IActionResult> RegisterStudents(Guid idGroup)
        {
            var clase = await _classService.ObtenerClasePorIdAsync(idGroup);
            if (!clase.IsSuccess || clase.Result is null)
            {
                TempData["ErrorClase"] = "La clase que intentas usar no existe";
                return RedirectToAction(nameof(ClassRegistrationFailed));
            }

            var estudiantes = await _classService.ObtenerTodosLosEstudiantesAsync();

            ViewBag.Clase = clase.Result;
            ViewBag.Estudiantes = estudiantes.Result ?? new List<StudentOptionDTO>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRoutePermission("/ClassSession/CreateClass", PermissionType.Create)]
        public async Task<IActionResult> RegisterStudentsDropdown(AddStudentsToGroupDTO model)
        {
            var resultado = await _classService.RegistrarEstudiantesAsync(model);

            if (!resultado.IsSuccess)
            {
                TempData["ErrorClase"] = resultado.Message;
                return RedirectToAction(nameof(ClassRegistrationFailed));
            }

            return RedirectToAction(nameof(ClassRegistrationSuccess), new { idGroup = model.IdGroup });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRoutePermission("/ClassSession/CreateClass", PermissionType.Create)]
        public async Task<IActionResult> RegisterStudentsFile(Guid idGroup, IFormFile archivo)
        {
            if (archivo is null || archivo.Length == 0)
            {
                TempData["ErrorClase"] = "Debes seleccionar un archivo .csv o .xlsx";
                return RedirectToAction(nameof(ClassRegistrationFailed));
            }

            await using var stream = archivo.OpenReadStream();
            var resultado = await _classService.RegistrarEstudiantesDesdeArchivoAsync(idGroup, stream, archivo.FileName);

            if (!resultado.IsSuccess || resultado.Result is null)
            {
                TempData["ErrorClase"] = resultado.Message;
                return RedirectToAction(nameof(ClassRegistrationFailed));
            }

            TempData["ResumenCarga"] =
                $"{resultado.Result.EstudiantesCreados} creados, " +
                $"{resultado.Result.EstudiantesVinculados} vinculados, " +
                $"{resultado.Result.FilasConError} con error.";

            return RedirectToAction(nameof(ClassRegistrationSuccess), new { idGroup });
        }

        // ---------- Paso 3: Notificación de resultado ----------

        [HttpGet]
        [RequireRoutePermission("/ClassSession/CreateClass", PermissionType.Create)]
        public async Task<IActionResult> ClassRegistrationSuccess(Guid idGroup)
        {
            var clase = await _classService.ObtenerClasePorIdAsync(idGroup);
            ViewBag.Clase = clase.Result;
            ViewBag.ResumenCarga = TempData["ResumenCarga"];
            return View();
        }

        [HttpGet]
        public IActionResult ClassRegistrationFailed()
        {
            ViewBag.Error = TempData["ErrorClase"] ?? "No fue posible completar el registro de la clase";
            return View();
        }

        // ---------- Paso 4: Listado, edición y desactivación ----------

        [HttpGet]
        [RequireRoutePermission("/ClassSession/ClassRecord", PermissionType.View)]
        public async Task<IActionResult> ClassRecord()
        {
            var idProfessor = ObtenerIdUsuarioActual();
            if (idProfessor is null)
                return RedirectToAction("LoginSelection", "Account");

            var clases = await _classService.ObtenerClasesDelDocenteAsync(idProfessor.Value);

            ViewBag.Mensaje = TempData["ClaseMensaje"];
            ViewBag.Error = TempData["ClaseError"];
            return View(clases.Result ?? new List<GroupSummaryDTO>());
        }

        [HttpGet]
        [RequireRoutePermission("/ClassSession/ClassRecord", PermissionType.Edit)]
        public async Task<IActionResult> EditClass(Guid id)
        {
            var clase = await _classService.ObtenerClasePorIdAsync(id);
            if (!clase.IsSuccess || clase.Result is null)
            {
                TempData["ClaseError"] = "La clase no existe";
                return RedirectToAction(nameof(ClassRecord));
            }

            var model = new EditClassDTO
            {
                IdGroup = clase.Result.IdGroup,
                GroupName = clase.Result.GroupName,
                Classroom = clase.Result.Classroom,
                SessionDate = clase.Result.ProximaSesion ?? DateTime.Today,
                StartTime = (clase.Result.ProximaSesion ?? DateTime.Today).TimeOfDay,
                EndTime = (clase.Result.ProximaSesion ?? DateTime.Today).TimeOfDay
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRoutePermission("/ClassSession/ClassRecord", PermissionType.Edit)]
        public async Task<IActionResult> EditClass(EditClassDTO model)
        {
            var resultado = await _classService.EditarClaseAsync(model);

            TempData[resultado.IsSuccess ? "ClaseMensaje" : "ClaseError"] = resultado.Message;
            return RedirectToAction(nameof(ClassRecord));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireRoutePermission("/ClassSession/ClassRecord", PermissionType.Delete)]
        public async Task<IActionResult> DeactivateClass(Guid id)
        {
            var resultado = await _classService.DesactivarClaseAsync(id);

            TempData[resultado.IsSuccess ? "ClaseMensaje" : "ClaseError"] = resultado.Message;
            return RedirectToAction(nameof(ClassRecord));
        }

        // ---------- Helper ----------

        private Guid? ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(idClaim, out var id) ? id : null;
        }


    }
}
