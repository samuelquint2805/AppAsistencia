using AppAsistencia.DTOs;
using AppAsistencia.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppAsistencia.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AcademicController : Controller
    {
        private readonly ISubjectService _subjectService;
        private readonly ICourseService _courseService;

        public AcademicController(ISubjectService subjectService, ICourseService courseService)
        {
            _subjectService = subjectService;
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var asignaturas = await _subjectService.ObtenerTodasAsync();
            var cursos = await _courseService.ObtenerTodosAsync();

            ViewBag.AsignaturasJson = System.Text.Json.JsonSerializer.Serialize(asignaturas.Result ?? new List<SubjectSummaryDTO>());
            ViewBag.CursosJson = System.Text.Json.JsonSerializer.Serialize(cursos.Result ?? new List<CourseSummaryDTO>());

            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.Error = TempData["Error"];
            ViewBag.Duplicados = TempData["Duplicados"];
            ViewBag.ErroresCarga = TempData["ErroresCarga"];

            return View();
        }
    }
}
