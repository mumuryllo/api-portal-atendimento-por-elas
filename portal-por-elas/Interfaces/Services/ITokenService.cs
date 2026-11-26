using portal_por_elas.Domain.Entities;
using System.Security.Claims;

namespace portal_por_elas.Interfaces.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiraEm) GenerateToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}