using AppAsistencia.Core;
using AppAsistencia.DTOs;
using AppAsistencia.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AppAsistencia.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _usuarioService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ITwoFactorService _twoFactorService;

        public AuthController(
            IUserService usuarioService,
            IJwtTokenService jwtTokenService,
            ITwoFactorService twoFactorService)
        {
            _usuarioService = usuarioService;
            _jwtTokenService = jwtTokenService;
            _twoFactorService = twoFactorService;
        }

        // POST api/auth/register
        [HttpPost("register")]
        [ProducesResponseType(typeof(Response<UserSummaryDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<UserSummaryDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Response<UserSummaryDTO>.Failure("Datos de registro inválidos"));

            var response = await _usuarioService.RegistrarUsuarioAsync(dto);

            if (!response.IsSuccess)
                return BadRequest(response);

            return CreatedAtAction(nameof(Register), new { id = response.Result!.IdUser }, response);
        }

        // POST api/auth/login
        // Paso 1: valida credenciales y, si son correctas, envia un codigo 2FA por correo.
        // NO entrega el JWT todavia.
        [HttpPost("login")]
        [ProducesResponseType(typeof(Response<TwoFactorRequiredDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<TwoFactorRequiredDTO>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Response<TwoFactorRequiredDTO>.Failure("Datos de inicio de sesión inválidos"));

            var credenciales = await _usuarioService.ValidarCredencialesAsync(dto.Email, dto.Password);

            if (!credenciales.IsSuccess || credenciales.Result is null)
                return Unauthorized(Response<TwoFactorRequiredDTO>.Failure(credenciales.Message ?? "Credenciales inválidas"));

            var user = credenciales.Result;

            var envio = await _twoFactorService.GenerarYEnviarCodigoAsync(user.idUser, user.email, user.firstname);
            if (!envio.IsSuccess)
                return StatusCode(500, Response<TwoFactorRequiredDTO>.Failure(envio.Message ?? "No se pudo enviar el código de verificación"));

            var resultado = new TwoFactorRequiredDTO
            {
                IdUser = user.idUser,
                EmailEnmascarado = EnmascararEmail(user.email)
            };

            return Ok(Response<TwoFactorRequiredDTO>.Success(resultado, "Revisa tu correo institucional para el código de verificación"));
        }

        // POST api/auth/verify-2fa
        // Paso 2: valida el codigo de 6 digitos. Si es correcto, AHI SI entrega el JWT.
        [HttpPost("verify-2fa")]
        [ProducesResponseType(typeof(Response<AuthResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<AuthResponseDTO>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] VerifyCodeDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Response<AuthResponseDTO>.Failure("Código inválido"));

            var validacion = _twoFactorService.ValidarCodigo(dto.IdUser, dto.Codigo);
            if (!validacion.IsSuccess)
                return Unauthorized(Response<AuthResponseDTO>.Failure(validacion.Message ?? "Código incorrecto"));

            var usuarioResponse = await _usuarioService.ObtenerPorIdAsync(dto.IdUser);
            if (!usuarioResponse.IsSuccess || usuarioResponse.Result is null)
                return Unauthorized(Response<AuthResponseDTO>.Failure("Usuario no encontrado"));

            var user = usuarioResponse.Result;
            var roleName = user.roleFK?.nombreRol ?? "Sin rol";
            var token = _jwtTokenService.GenerateToken(user, roleName, out var expiresAt);

            var authDto = new AuthResponseDTO
            {
                IdUser = user.idUser,
                UserName = user.userName,
                Email = user.email,
                Role = roleName,
                Token = token,
                ExpiresAt = expiresAt
            };

            return Ok(Response<AuthResponseDTO>.Success(authDto, "Inicio de sesión exitoso"));
        }

        // POST api/auth/resend-2fa
        // Por si el usuario no recibio el correo a tiempo y necesita un codigo nuevo.
        [HttpPost("resend-2fa")]
        public async Task<IActionResult> ResendTwoFactor([FromBody] TwoFactorRequiredDTO dto)
        {
            var usuarioResponse = await _usuarioService.ObtenerPorIdAsync(dto.IdUser);
            if (!usuarioResponse.IsSuccess || usuarioResponse.Result is null)
                return NotFound(Response<bool>.Failure("Usuario no encontrado"));

            var user = usuarioResponse.Result;
            var envio = await _twoFactorService.GenerarYEnviarCodigoAsync(user.idUser, user.email, user.firstname);

            return envio.IsSuccess ? Ok(envio) : StatusCode(500, envio);
        }

        private static string EnmascararEmail(string email)
        {
            var partes = email.Split('@');
            if (partes.Length != 2 || partes[0].Length < 2) return email;
            var visible = partes[0][..2];
            return $"{visible}{new string('*', partes[0].Length - 2)}@{partes[1]}";
        }
    }
}