using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppAsistencia.Controllers
{
    public class AttendanceRecordController : Controller
    {
        // GET: AttendanceRecordController
        public ActionResult attendanceConfirm()
        {
            return View();
        }

       
        public IActionResult Index()
        {
            return View();
        }

        // GET: AttendanceRecordController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AttendanceRecordController/Create
        public ActionResult CreateAssRecord()
        {
            return View();
        }

        public ActionResult CreateManualAssRecord()
        {
            return View();
        }

        public ActionResult ViewFaults()
        {
            return View();
        }

        // POST: AttendanceRecordController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AttendanceRecordController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AttendanceRecordController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: AttendanceRecordController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AttendanceRecordController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
