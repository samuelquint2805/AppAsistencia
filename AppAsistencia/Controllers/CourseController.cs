using AppAsistencia.DTOs;
using AppAsistencia.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppAsistencia.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ISubjectService _subjectService;

        public CourseController(ICourseService courseService, ISubjectService subjectService)
        {
            _courseService = courseService;
            _subjectService = subjectService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cursos = await _courseService.ObtenerTodosAsync();

            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.Error = TempData["Error"];
            ViewBag.Duplicados = TempData["Duplicados"];
            ViewBag.ErroresCarga = TempData["ErroresCarga"];

            return View(cursos.Result ?? new List<CourseSummaryDTO>());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarListasDesplegablesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseDTO model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los campos del formulario, incluyendo al menos un día de clase";
                return RedirectToAction("Index", "Academic");
            }

            var resultado = await _courseService.CrearCursoAsync(model);

            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return resultado.IsSuccess ? RedirectToAction("Index", "Academic") : RedirectToAction(nameof(Create));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(IFormFile archivo)
        {
            if (archivo is null || archivo.Length == 0)
            {
                TempData["Error"] = "Debes seleccionar un archivo .csv o .xlsx";
                return RedirectToAction("Index", "Academic");
            }

            await using var stream = archivo.OpenReadStream();
            var resultado = await _courseService.CargarCursosMasivoAsync(stream, archivo.FileName);

            if (!resultado.IsSuccess || resultado.Result is null)
            {
                TempData["Error"] = resultado.Message;
                return RedirectToAction("Index", "Academic");
            }

            TempData["Mensaje"] = $"{resultado.Result.Creados} curso(s) creado(s)";
            if (resultado.Result.Duplicados.Count > 0)
                TempData["Duplicados"] = string.Join(" | ", resultado.Result.Duplicados);
            if (resultado.Result.Errores.Count > 0)
                TempData["ErroresCarga"] = string.Join(" | ", resultado.Result.Errores);

            return RedirectToAction("Index", "Academic");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var curso = await _courseService.ObtenerPorIdAsync(id);
            if (!curso.IsSuccess || curso.Result is null)
            {
                TempData["Error"] = "El curso no existe";
                return RedirectToAction("Index", "Academic");
            }

            await CargarListasDesplegablesAsync();

            var model = new EditCourseDTO
            {
                IdGroup = curso.Result.IdGroup,
                GroupCode = curso.Result.GroupCode,
                GroupName = curso.Result.GroupName,
                Classroom = curso.Result.Classroom,
                ProfessorId = Guid.Empty, // se resuelve en la vista comparando por nombre si hace falta
                Dias = curso.Result.Dias
            };

            ViewBag.Curso = curso.Result;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditCourseDTO model)
        {
            var resultado = await _courseService.EditarCursoAsync(model);

            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return resultado.IsSuccess
                ? RedirectToAction("Index", "Academic")
                : RedirectToAction(nameof(Edit), new { id = model.IdGroup });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(Guid id)
        {
            var resultado = await _courseService.DeshabilitarCursoAsync(id);
            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return RedirectToAction("Index", "Academic");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable(Guid id)
        {
            var resultado = await _courseService.HabilitarCursoAsync(id);
            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return RedirectToAction("Index", "Academic");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var resultado = await _courseService.EliminarCursoAsync(id);
            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return RedirectToAction("Index", "Academic");
        }

        // ---------------- Helper ----------------

        private async Task CargarListasDesplegablesAsync()
        {
            var asignaturas = await _subjectService.ObtenerTodasAsync();
            var profesores = await _courseService.ObtenerProfesoresDisponiblesAsync();

            ViewBag.Asignaturas = (asignaturas.Result ?? new List<SubjectSummaryDTO>())
                .Where(a => a.IsActive).ToList();
            ViewBag.Profesores = profesores.Result ?? new List<ProfessorOptionDTO>();
        }
    }
}
