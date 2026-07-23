using AppAsistencia.Models;

namespace AppAsistencia.Services.Abstractions
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, string roleName, out DateTime expiresAt);
    }
}
