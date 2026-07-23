using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppAsistencia.Controllers
{
    public class ClassSessionController : Controller
    {
        // GET: ClassSessionController
        public ActionResult ClassRecord()
        {
            return View();
        }

        // GET: ClassSessionController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ClassSessionController/Create
        public ActionResult CreateClass()
        {
            return View();
        }



        // POST: ClassSessionController/Create
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

        // GET: ClassSessionController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ClassSessionController/Edit/5
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

        // GET: ClassSessionController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ClassSessionController/Delete/5
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
