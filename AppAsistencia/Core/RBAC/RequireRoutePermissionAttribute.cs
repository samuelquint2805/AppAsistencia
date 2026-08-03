using AppAsistencia.Core;
using AppAsistencia.Data.DBSET;
using AppAsistencia.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AppAsistencia.Core.RBAC
{
    // Reemplaza [Authorize(Roles = "...")] cuando el permiso NO es fijo en el código,
    // sino que depende de lo que el Administrador configuró en ROUTEROLEMANAGEMENT.
    //
    // Uso(ejemplo): [RequireRoutePermission("/AttendanceRecord/attendanceConfirm", PermissionType.Edit)]
    //
    // routeName debe coincidir EXACTAMENTE con el valor sembrado en RoutesAs.routeName
    // (el mismo que usaste en tu RoleRouteSeeder).
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class RequireRoutePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _routeName;
        private readonly PermissionType[] _permisos;

        public RequireRoutePermissionAttribute(string routeName, params PermissionType[] permisos)
        {
            _routeName = routeName;
            _permisos = permisos;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // 1. Debe estar autenticado (misma cookie que usa Verificar2FA)
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
                return;
            }

            var roleName = user.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrWhiteSpace(roleName))
            {
                context.Result = new ForbidResult(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            // 2. Consulta en vivo contra la BD: ¿este rol tiene el permiso pedido en esta ruta?
            var db = context.HttpContext.RequestServices.GetRequiredService<DataContextAsistencia>();

            var routeRole = await db.Set<RouteRole>()
                .Include(rr => rr.route)
                .Include(rr => rr.role)
                .FirstOrDefaultAsync(rr => rr.route.routeName == _routeName && rr.role.nombreRol == roleName);

            if (routeRole is null)
            {
                context.Result = new ForbidResult(CookieAuthenticationDefaults.AuthenticationScheme);
                return;
            }

            // 3. Evaluar si el usuario cumple con los permisos requeridos
            // (Regla: basta con que tenga al menos uno de los permisos especificados en el atributo)
            bool tienePermiso = false;
            foreach (var permiso in _permisos)
            {
                var cumpleActual = permiso switch
                {
                    PermissionType.View => routeRole.canView,
                    PermissionType.Create => routeRole.canCreate,
                    PermissionType.Edit => routeRole.canEdit,
                    PermissionType.Delete => routeRole.canDelete,
                    _ => false
                };
                if (cumpleActual)
                {
                    tienePermiso = true;
                    break; // Ya tiene al menos uno de los permisos permitidos
                }
            }
            if (!tienePermiso)
            {
                context.Result = new ForbidResult(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        }
    }
}
