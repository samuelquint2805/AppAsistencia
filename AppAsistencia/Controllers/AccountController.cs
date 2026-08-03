using AppAsistencia.Core.RBAC;
using AppAsistencia.DTOs;
using AppAsistencia.Models;
using AppAsistencia.Services.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

        // ==========================================
        // VISTAS DE SELECCIÓN Y LOGIN (GET)
        // ==========================================
        public IActionResult LoginSelection()
        {
            return View();
        }
        [HttpGet]
        public IActionResult LoginEstudiante()
        {
            return View();
        }
        [HttpGet]
        public IActionResult LoginDocente()
        {
            return View();
        }
        [HttpGet]
        public IActionResult LoginAdmin()
        {
            return View();
        }
        // ==========================================
        // ACCIONES DE LOGIN (POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
       
        public async Task<IActionResult> LoginEstudiante(LoginFormModel model)
        {
            return await ProcesarLogin(model, RoleType.Student, "~/Views/Account/LoginEstudiante.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> LoginDocente(LoginFormModel model)
        {
            return await ProcesarLogin(model, RoleType.Professor, "~/Views/Account/LoginDocente.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> LoginAdmin(LoginFormModel model)
        {
            return await ProcesarLogin(model, RoleType.Administrator, "~/Views/Account/LoginAdmin.cshtml");
        }

        /// <summary>
        /// Método genérico para validar credenciales, confirmación de correo y rol
        /// </summary>
        private async Task<IActionResult> ProcesarLogin(LoginFormModel model, RoleType rolEsperado, string vistaRetorno)
        {
            if (!ModelState.IsValid)
            {
                return View(vistaRetorno, model);
            }
            // 1. Validar credenciales (email y contraseña)
            var validacion = await _usuarioService.ValidarCredencialesAsync(model.Email, model.Password);
            if (!validacion.IsSuccess || validacion.Result is null)
            {
                ViewBag.Error = validacion.Message;
                return View(vistaRetorno, model);
            }
            var user = validacion.Result;
            // 2. Verificar que el rol coincida con el formulario utilizado
            var nombreRolUsuario = user.roleFK?.nombreRol ?? string.Empty;
            if (!nombreRolUsuario.Equals(rolEsperado.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.Error = $"Esta cuenta no corresponde al rol de {rolEsperado}.";
                return View(vistaRetorno, model);
            }
            // 3. REGLA DE NEGOCIO: Si el correo NO está confirmado -> No dejar loguear y enviar a 2FA
            if (!user.isEmailConfirmed)
            {
                // Generar y enviar el código 2FA al correo institucional
                var resultado2FA = await _twoFactorService.GenerarYEnviarCodigoAsync(user.idUser, user.email, user.firstname);
                if (!resultado2FA.IsSuccess)
                {
                    ViewBag.Error = "Su correo no está verificado y no se pudo enviar el código de seguridad. Intente nuevamente.";
                    return View(vistaRetorno, model);
                }
                // Redirigir a la vista de verificación 2FA
                TempData["InfoMessage"] = "Tu correo no ha sido verificado. Te hemos enviado un código de 6 dígitos a tu email.";
                return RedirectToAction(nameof(Verificacion2FA), new { idUser = user.idUser, email = user.email });
            }
            // 4. Si el correo YA está verificado -> Iniciar Sesión inmediatamente
            await IniciarSesionCookie(user);
            return RedirectToAction(nameof(LoginExitoso));
        }

        [HttpGet]
       
        public IActionResult RegisterStudent()
        {
            // return await Register(model, esDocente: false);
            return View();
        }

       

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterStudent(RegisterFormModel model)
        {
             return await Register(model, esDocente: false);
           
        }

        [HttpGet]
        public IActionResult RegisterProfessor()
        {
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> RegisterProfessor(RegisterFormModel model)
        {
             return await Register(model, esDocente: true);
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterFormModel model, bool esDocente)
        {
            if (model.Password != model.ConfirmPassword)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                return View(esDocente ? "~/Views/Account/RegisterProfessor.cshtml" : "~/Views/Account/RegisterStudent.cshtml");
            }

            var nombreCompleto = model.Name?.Trim() ?? string.Empty;
            var espacio = nombreCompleto.IndexOf(' ');
            var firstName = espacio > 0 ? nombreCompleto[..espacio] : nombreCompleto;
            var lastName = espacio > 0 ? nombreCompleto[(espacio + 1)..] : string.Empty;

            var dto = new RegisterDTO
            {
               
                UserName = model.Email.Split('@')[0],
                FirstName = firstName,
                LastName = lastName,
                Email = model.Email,
                Password = model.Password,
                Role = esDocente ? RoleType.Professor : RoleType.Student,   // <- decidido aquí, no por el usuario
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
                return View(esDocente ? "~/Views/Account/RegisterProfessor.cshtml" : "~/Views/Account/RegisterStudent.cshtml");
            }

            // A partir de aquí arranca el mismo flujo 2FA que usa el login
            var idUser = resultado.Result!.IdUser;
            var envio = await _twoFactorService.GenerarYEnviarCodigoAsync(idUser, dto.Email, dto.FirstName);

            if (!envio.IsSuccess)
            {
                TempData["RegistroExitoso"] = "Tu cuenta se creó, pero no pudimos enviar el código. Inicia sesión para reintentarlo.";
                return RedirectToAction(nameof(LoginSelection));
            }

            return RedirectToAction(nameof(Verificacion2FA), new { idUser, email = EnmascararEmail(dto.Email) });
        }

        private static string EnmascararEmail(string email)
        {
            var partes = email.Split('@');
            if (partes.Length != 2 || partes[0].Length < 2) return email;
            var visible = partes[0][..2];
            return $"{visible}{new string('*', partes[0].Length - 2)}@{partes[1]}";
        }

        //GENERACION DE CODIGOS Y VERIFICACIONES


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

            // Si el código llegó como parte del registro, esto confirma el correo institucional
            if (!user.isEmailConfirmed)
            {
                await _usuarioService.ConfirmarEmailDirectoAsync(idUser);
            }

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

        // ==========================================
        // MÉTODOS AUXILIARES Y LOGOUT
        // ==========================================
        private async Task IniciarSesionCookie(User user)
        {
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
        }

        [HttpGet]
        
        public IActionResult LoginExitoso()
        {
            return View();
        }

        

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


        [Authorize]
        [HttpGet]
        [RequireRoutePermission("/Home/Index", PermissionType.View, PermissionType.Edit, PermissionType.Delete)]
        public async Task<IActionResult> ConfigurationPage()
        {
            var idUserClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idUserClaim, out var idUser))
                return RedirectToAction(nameof(LoginSelection));

            var usuarioResponse = await _usuarioService.ObtenerPorIdAsync(idUser);
            if (!usuarioResponse.IsSuccess || usuarioResponse.Result is null)
                return RedirectToAction(nameof(LoginSelection));

            var user = usuarioResponse.Result;
            var roleName = user.roleFK?.nombreRol;

            ViewBag.Mensaje = TempData["ConfiguracionExitosa"];
            ViewBag.Error = TempData["ConfiguracionError"];

            // ---------- Student ----------
            if (roleName == "Student")
            {
                var estudianteResponse = await _usuarioService.ObtenerEstudiantePorIdAsync(idUser);
                if (!estudianteResponse.IsSuccess || estudianteResponse.Result is null)
                    return RedirectToAction(nameof(LoginSelection));

                var estudiante = estudianteResponse.Result;

                var modelEstudiante = new ConfigurationStudentViewModel
                {
                    IdUser = user.idUser,
                    FullName = $"{user.firstname} {user.lastName}".Trim(),
                    Email = user.email,
                    StudentIdCard = estudiante.studentIdCard,
                    CurrentSemester = estudiante.currentSemester,
                    PhoneNumber = estudiante.phoneNumber
                };

                return View("~/Views/Account/ConfigurationPageStudent.cshtml", modelEstudiante);
            }

            // ---------- Professor o Administrator (vista compartida) ----------
            var modelStaff = new ConfigurationStaffViewModel
            {
                IdUser = user.idUser,
                FullName = $"{user.firstname} {user.lastName}".Trim(),
                Email = user.email,
                EsProfessor = roleName == "Professor"
            };

            if (roleName == "Professor")
            {
                var profesorResponse = await _usuarioService.ObtenerProfesorPorIdAsync(idUser);
                if (profesorResponse.IsSuccess && profesorResponse.Result is not null)
                {
                    modelStaff.ProfessorIdCard = profesorResponse.Result.professorIdCard;
                    modelStaff.Department = profesorResponse.Result.department;
                    modelStaff.PhoneNumber = profesorResponse.Result.phoneNumber;
                }
            }
            else // Administrator
            {
                var adminResponse = await _usuarioService.ObtenerAdministradorPorIdAsync(idUser);
                if (adminResponse.IsSuccess && adminResponse.Result is not null)
                {
                    modelStaff.PhoneNumber = adminResponse.Result.phoneNumber;
                }
            }

            return View("~/Views/Account/ConfigurationPageStaff.cshtml", modelStaff);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarConfiguracion(ConfigurationFormModel model)
        {
            var idUserClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(idUserClaim, out var idUser))
                return RedirectToAction(nameof(LoginSelection));

            var usuarioResponse = await _usuarioService.ObtenerPorIdAsync(idUser);
            if (!usuarioResponse.IsSuccess || usuarioResponse.Result is null)
                return RedirectToAction(nameof(LoginSelection));

            var roleName = usuarioResponse.Result.roleFK?.nombreRol;

            // 1. Perfil (nombre + correo), comun a los 3 roles
            var nombreCompleto = model.Name?.Trim() ?? string.Empty;
            var espacio = nombreCompleto.IndexOf(' ');
            var firstName = espacio > 0 ? nombreCompleto[..espacio] : nombreCompleto;
            var lastName = espacio > 0 ? nombreCompleto[(espacio + 1)..] : string.Empty;

            var perfilResultado = await _usuarioService.ActualizarPerfilAsync(idUser, firstName, lastName, model.Email);
            if (!perfilResultado.IsSuccess)
            {
                TempData["ConfiguracionError"] = perfilResultado.Message;
                return RedirectToAction(nameof(ConfigurationPage));
            }

            // 2. Datos especificos segun el rol real del usuario (no lo que mande el formulario)
            switch (roleName)
            {
                case "Student":
                    if (model.CurrentSemester is not null)
                    {
                        var resultadoEstudiante = await _usuarioService.ActualizarDatosEstudianteAsync(
                            idUser, model.PhoneNumber, model.CurrentSemester.Value, model.StudentIdCard);

                        if (!resultadoEstudiante.IsSuccess)
                        {
                            TempData["ConfiguracionError"] = resultadoEstudiante.Message;
                            return RedirectToAction(nameof(ConfigurationPage));
                        }
                    }
                    break;

                case "Professor":
                    var resultadoProfesor = await _usuarioService.ActualizarDatosProfesorAsync(
                        idUser, model.PhoneNumber, model.Department, model.ProfessorIdCard);

                    if (!resultadoProfesor.IsSuccess)
                    {
                        TempData["ConfiguracionError"] = resultadoProfesor.Message;
                        return RedirectToAction(nameof(ConfigurationPage));
                    }
                    break;

                case "Administrator":
                    var resultadoAdmin = await _usuarioService.ActualizarDatosAdministradorAsync(idUser, model.PhoneNumber);

                    if (!resultadoAdmin.IsSuccess)
                    {
                        TempData["ConfiguracionError"] = resultadoAdmin.Message;
                        return RedirectToAction(nameof(ConfigurationPage));
                    }
                    break;
            }

            // 3. Cambio de contraseña (opcional: solo si el usuario llenó los campos)
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                if (model.NewPassword != model.ConfirmNewPassword)
                {
                    TempData["ConfiguracionError"] = "La nueva contraseña y su confirmación no coinciden";
                    return RedirectToAction(nameof(ConfigurationPage));
                }

                var cambioPassword = await _usuarioService.CambiarContrasenaAsync(
                    idUser, model.CurrentPassword ?? string.Empty, model.NewPassword);

                if (!cambioPassword.IsSuccess)
                {
                    TempData["ConfiguracionError"] = cambioPassword.Message;
                    return RedirectToAction(nameof(ConfigurationPage));
                }
            }

            // 4. Refresca los claims de la cookie (nombre/email pudieron cambiar)
            var usuarioActualizado = await _usuarioService.ObtenerPorIdAsync(idUser);
            if (usuarioActualizado.IsSuccess && usuarioActualizado.Result is not null)
            {
                var user = usuarioActualizado.Result;
                var roleNameActualizado = user.roleFK?.nombreRol ?? "Sin rol";

                var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.idUser.ToString()),
            new(ClaimTypes.Name, user.userName),
            new(ClaimTypes.Email, user.email),
            new(ClaimTypes.Role, roleNameActualizado)
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });
            }

            TempData["ConfiguracionExitosa"] = "Cambios guardados correctamente";
            return RedirectToAction(nameof(ConfigurationPage));
        }


        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
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
