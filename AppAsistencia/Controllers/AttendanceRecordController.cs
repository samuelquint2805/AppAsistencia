using AppAsistencia.Core.RBAC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppAsistencia.Controllers
{
    public class AttendanceRecordController : Controller
    {
        // GET: AttendanceRecordController
        [HttpGet]
        [RequireRoutePermission("/AttendanceRecord/attendanceConfirm", PermissionType.View, PermissionType.Edit)]
        public ActionResult attendanceConfirm()
        {
            return View();
        }

       

        // GET: AttendanceRecordController/Create
        [HttpGet]
        [RequireRoutePermission("/AttendanceRecord/CreateAssRecord", PermissionType.View, PermissionType.Create)]
        public ActionResult CreateAssRecord()
        {
            return View();
        }

        [HttpGet]
        [RequireRoutePermission("/AttendanceRecord/CreateManualAssRecord", PermissionType.View, PermissionType.Create)]
        public ActionResult CreateManualAssRecord()
        {
            return View();
        }

        [HttpGet]
        [RequireRoutePermission("/AttendanceRecord/ViewFaults", PermissionType.View, PermissionType.Edit, PermissionType.Delete)]
        public ActionResult ViewFaults()
        {
            return View();
        }

        

        
    }
}
