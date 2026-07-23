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

        public AuthController(IUserService usuarioService, IJwtTokenService jwtTokenService)
        {
            _usuarioService = usuarioService;
            _jwtTokenService = jwtTokenService;
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
        [HttpPost("login")]
        [ProducesResponseType(typeof(Response<AuthResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<AuthResponseDTO>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Response<AuthResponseDTO>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(Response<AuthResponseDTO>.Failure("Datos de inicio de sesión inválidos"));

            var credenciales = await _usuarioService.ValidarCredencialesAsync(dto.Email, dto.Password);

            if (!credenciales.IsSuccess || credenciales.Result is null)
                return Unauthorized(Response<AuthResponseDTO>.Failure(credenciales.Message ?? "Credenciales inválidas"));

            var user = credenciales.Result;
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
    }
}