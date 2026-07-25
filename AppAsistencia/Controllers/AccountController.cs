using AppAsistencia.DTOs;
using AppAsistencia.Models;
using AppAsistencia.Services.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppAsistencia.Controllers
{
    public class AccountController : Controller
    {

        private readonly IUserService _usuarioService;
        private readonly ITwoFactorService _twoFactorService;

        public AccountController(IUserService usuarioService, ITwoFactorService twoFactorService)
        {
            _usuarioService = usuarioService;
            _twoFactorService = twoFactorService;
        }

        // GET: AccountController
        public IActionResult LoginSelection()
        {
            return View();
        }

        public IActionResult Verification2FA()
        {
            return View();
        }

        public IActionResult LoginEstudiante()
        {
            return View();
        }

        public IActionResult LoginDocente()
        {
            return View();
        }

        

        [HttpGet]
        public IActionResult RegisterPage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterFormModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View("~/Views/Account/RegisterPage.cshtml");
            }

            var nombreCompleto = model.Name?.Trim() ?? string.Empty;
            var espacio = nombreCompleto.IndexOf(' ');
            var firstName = espacio > 0 ? nombreCompleto[..espacio] : nombreCompleto;
            var lastName = espacio > 0 ? nombreCompleto[(espacio + 1)..] : string.Empty;

            var esDocente = model.UserType == "docente";

            var dto = new RegisterDTO
            {
                InstitutionalCode = model.InstitutionalCode,
                UserName = model.Email.Split('@')[0],
                FirstName = firstName,
                LastName = lastName,
                Email = model.Email,
                Password = model.Password,
                Role = esDocente ? RoleType.Professor : RoleType.Student,
                studentIdCard = model.StudentIdCard,
                currentSemester = model.CurrentSemester,
                professorIdCard = model.TeacherCard,
                department = model.Department,
                phoneNumber = esDocente ? model.TeacherPhone : model.phoneNumber
            };

            var resultado = await _usuarioService.RegistrarUsuarioAsync(dto);

            if (!resultado.IsSuccess)
            {
                ViewBag.Error = resultado.Message;
                return View("~/Views/Account/RegisterPage.cshtml");
            }

            TempData["RegistroExitoso"] = "Cuenta creada correctamente. Ya puedes iniciar sesión.";
            return RedirectToAction(nameof(LoginSelection));
        }
        [HttpGet]
        public IActionResult Verificacion2FA(Guid idUser, string email)
        {
            ViewBag.IdUser = idUser;
            ViewBag.UserEmail = string.IsNullOrWhiteSpace(email) ? "tu correo institucional" : email;
            return View("~/Views/Account/Verificacion2FA.cshtml");
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verificar2FA(Guid idUser, List<string> otpDigits)
        {
            var codigo = string.Join(string.Empty, otpDigits ?? new List<string>());

            var validacion = _twoFactorService.ValidarCodigo(idUser, codigo);
            if (!validacion.IsSuccess)
            {
                ViewBag.IdUser = idUser;
                ViewBag.UserEmail = "tu correo institucional";
                ViewBag.Error = validacion.Message;
                return View("~/Views/Account/Verificacion2FA.cshtml");
            }

            var usuarioResponse = await _usuarioService.ObtenerPorIdAsync(idUser);
            if (!usuarioResponse.IsSuccess || usuarioResponse.Result is null)
            {
                ViewBag.Error = "Usuario no encontrado";
                return View("~/Views/Account/Verificacion2FA.cshtml");
            }

            var user = usuarioResponse.Result;
            var roleName = user.roleFK?.nombreRol ?? "Sin rol";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.idUser.ToString()),
                new(ClaimTypes.Name, user.userName),
                new(ClaimTypes.Email, user.email),
                new(ClaimTypes.Role, roleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

            return RedirectToAction(nameof(LoginExitoso));
        }

        [HttpGet]
        public IActionResult LoginExitoso()
        {
            return View();
        }

        // ---------- Logout ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(LoginSelection));
        }

        private static int? ParseIntOrNull(string? valor)
        {
            return int.TryParse(valor, out var resultado) ? resultado : null;
        }
        public IActionResult ConfigurationPage()
        {
            return View();
        }
        

        public IActionResult ForgotPassword()
        {
            return View();
        }

       
       

       

        // GET: AccountController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AccountController/Delete/5
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
