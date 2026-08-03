using AppAsistencia.Core;
using AppAsistencia.Core.RBAC;
using AppAsistencia.Data.DBSET;
using AppAsistencia.DTOs;
using AppAsistencia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppAsistencia.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class RoleController : Controller
    {
        private readonly DataContextAsistencia _context;

        public RoleController(DataContextAsistencia context)
        {
            _context = context;
        }

        // GET: Role/RolePermissions
        [HttpGet]
        [RequireRoutePermission("/Role/RolePermissions", PermissionType.Edit)]
        public async Task<IActionResult> RolePermissions()
        {
            var permisosDocente = await CargarSeccionesAsync("docente");
            var permisosEstudiante = await CargarSeccionesAsync("estudiante");

            ViewBag.PermisosDocenteJson = System.Text.Json.JsonSerializer.Serialize(permisosDocente);
            ViewBag.PermisosEstudianteJson = System.Text.Json.JsonSerializer.Serialize(permisosEstudiante);

            return View();
        }

        // POST: Role/SavePermissions
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SavePermissions([FromBody] RolePermissionsFormModel model)
        {
            var roleName = CatalogoPermisos.RoleNameDesdeSlug(model.RoleName);

            var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.nombreRol == roleName);
            if (role is null)
                return NotFound(new { message = $"El rol '{roleName}' no existe" });

            foreach (var seccion in model.Secciones)
            {
                var info = CatalogoPermisos.Secciones.FirstOrDefault(s => s.Slug == seccion.Slug);
                if (info is null) continue; // slug desconocido: se ignora, no se inventan rutas nuevas

                var ruta = await _context.Set<RoutesAs>().FirstOrDefaultAsync(r => r.routeName == info.RouteName);
                if (ruta is null) continue; // la ruta debe existir previamente (via el seeder)

                var routeRole = await _context.Set<RouteRole>()
                    .FirstOrDefaultAsync(rr => rr.routeID == ruta.idRoute && rr.roleID == role.idRol);

                // El admin solo puede HABILITAR permisos existentes, no crear rutas/roles nuevos.
                // Si "activa" está apagado, se apagan también las 4 acciones (sin permiso = sin acceso).
                var ver = seccion.Activa && seccion.Ver;
                var crear = seccion.Activa && seccion.Crear;
                var editar = seccion.Activa && seccion.Editar;
                var eliminar = seccion.Activa && seccion.Eliminar;

                if (routeRole is null)
                {
                    _context.Add(new RouteRole
                    {
                        routeID = ruta.idRoute,
                        roleID = role.idRol,
                        canView = ver,
                        canCreate = crear,
                        canEdit = editar,
                        canDelete = eliminar
                    });
                }
                else
                {
                    routeRole.canView = ver;
                    routeRole.canCreate = crear;
                    routeRole.canEdit = editar;
                    routeRole.canDelete = eliminar;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Permisos guardados correctamente" });
        }

        private async Task<List<object>> CargarSeccionesAsync(string rolSlug)
        {
            var roleName = CatalogoPermisos.RoleNameDesdeSlug(rolSlug);
            var role = await _context.Set<Role>().FirstOrDefaultAsync(r => r.nombreRol == roleName);
            var routeRoles = role is null
                ? new List<RouteRole>()
                : await _context.Set<RouteRole>()
                    .Include(rr => rr.route)
                    .Where(rr => rr.roleID == role.idRol)
                    .ToListAsync();
            var resultado = new List<object>();
            foreach (var info in CatalogoPermisos.Secciones)
            {
                var existente = routeRoles.FirstOrDefault(rr => rr.route.routeName == info.RouteName);
                var activa = existente is not null &&
                             (existente.canView || existente.canCreate || existente.canEdit || existente.canDelete);
                resultado.Add(new
                {
                    id = info.Slug,
                    nombre = info.Nombre,
                    descripcion = info.Descripcion,
                    soporta = new
                    {
                        ver = info.SoportaVer,
                        crear = info.SoportaCrear,
                        editar = info.SoportaEditar,
                        eliminar = info.SoportaEliminar
                    },
                    activa,
                    permisos = new
                    {
                        ver = existente?.canView ?? false,
                        crear = existente?.canCreate ?? false,
                        editar = existente?.canEdit ?? false,
                        eliminar = existente?.canDelete ?? false
                    },
                    expandida = false
                });
            }
            return resultado;
        }
    }
}