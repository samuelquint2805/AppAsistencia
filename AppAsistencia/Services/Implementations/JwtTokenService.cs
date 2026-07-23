using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppAsistencia.Models;
using AppAsistencia.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AppAsistencia.Services.Implementations
{
    // Requiere en appsettings.json:
    // "Jwt": {
    //   "Key": "UNA_CLAVE_SECRETA_LARGA_DE_AL_MENOS_32_CARACTERES",
    //   "Issuer": "AppAsistencia",
    //   "Audience": "AppAsistenciaClientes",
    //   "ExpireMinutes": "120"
    // }
    //
    // Paquetes NuGet necesarios:
    //   System.IdentityModel.Tokens.Jwt
    //   Microsoft.IdentityModel.Tokens
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user, string roleName, out DateTime expiresAt)
        {
            var jwt = _configuration.GetSection("Jwt");

            var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key no está configurado");
            var issuer = jwt["Issuer"] ?? "AppAsistencia";
            var audience = jwt["Audience"] ?? "AppAsistenciaClientes";
            var expireMinutes = int.Parse(jwt["ExpireMinutes"] ?? "120");

            expiresAt = DateTime.UtcNow.AddMinutes(expireMinutes);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.idUser.ToString()),
                new(JwtRegisteredClaimNames.Email, user.email),
                new(ClaimTypes.Name, user.userName),
                new(ClaimTypes.Role, roleName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}