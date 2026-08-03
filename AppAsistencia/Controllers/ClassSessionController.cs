using AppAsistencia.Core.RBAC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppAsistencia.Controllers
{
    public class ClassSessionController : Controller
    {
        // GET: ClassSessionController
        [HttpGet]
        [RequireRoutePermission("/ClassSession/ClassRecord", PermissionType.Edit, PermissionType.Delete, PermissionType.View)]
        public ActionResult ClassRecord()
        {
            return View();
        }


        [HttpGet]
        [RequireRoutePermission("/ClassSession/CreateClass", PermissionType.Edit, PermissionType.Create, PermissionType.Delete, PermissionType.View)]
        // GET: ClassSessionController/Create
        public ActionResult CreateClass()
        {
            return View();
        }


    }
}
