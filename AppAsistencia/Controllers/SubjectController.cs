using AppAsistencia.DTOs;
using AppAsistencia.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppAsistencia.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SubjectController : Controller
    {
        private readonly ISubjectService _subjectService;

        public SubjectController(ISubjectService subjectService)
        {
            _subjectService = subjectService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var asignaturas = await _subjectService.ObtenerTodasAsync();

            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.Error = TempData["Error"];
            ViewBag.Duplicados = TempData["Duplicados"];
            ViewBag.ErroresCarga = TempData["ErroresCarga"];

            return View(asignaturas.Result ?? new List<SubjectSummaryDTO>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubjectDTO model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Revisa los campos del formulario";
                return RedirectToAction("Index", "Academic");
            }

            var resultado = await _subjectService.CrearAsignaturaAsync(model);

            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return RedirectToAction("Index", "Academic");
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
            var resultado = await _subjectService.CargarAsignaturasMasivoAsync(stream, archivo.FileName);

            if (!resultado.IsSuccess || resultado.Result is null)
            {
                TempData["Error"] = resultado.Message;
                return RedirectToAction("Index", "Academic");
            }

            TempData["Mensaje"] = $"{resultado.Result.Creados} asignatura(s) creada(s)";
            if (resultado.Result.Duplicados.Count > 0)
                TempData["Duplicados"] = string.Join(" | ", resultado.Result.Duplicados);
            if (resultado.Result.Errores.Count > 0)
                TempData["ErroresCarga"] = string.Join(" | ", resultado.Result.Errores);

            return RedirectToAction("Index", "Academic");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var asignatura = await _subjectService.ObtenerPorIdAsync(id);
            if (!asignatura.IsSuccess || asignatura.Result is null)
            {
                TempData["Error"] = "La asignatura no existe";
                return RedirectToAction("Index", "Academic");
            }

            var model = new EditSubjectDTO
            {
                IdSubject = asignatura.Result.IdSubject,
                SubjectCode = asignatura.Result.SubjectCode,
                Name = asignatura.Result.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditSubjectDTO model)
        {
            var resultado = await _subjectService.EditarAsignaturaAsync(model);

            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return RedirectToAction("Index", "Academic");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable(Guid id)
        {
            var resultado = await _subjectService.DeshabilitarAsignaturaAsync(id);
            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return RedirectToAction("Index", "Academic");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enable(Guid id)
        {
            var resultado = await _subjectService.HabilitarAsignaturaAsync(id);
            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return RedirectToAction("Index", "Academic");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var resultado = await _subjectService.EliminarAsignaturaAsync(id);
            TempData[resultado.IsSuccess ? "Mensaje" : "Error"] = resultado.Message;
            return RedirectToAction("Index", "Academic");
        }
    }
}
