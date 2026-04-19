using Auth.Domain.Entities;
using System.Security.Claims;

namespace Auth.Application.Interfaces
{
    /// <summary>
    /// Interface for JWT token operations.
    /// Defined in Application, implemented in Infrastructure.
    /// </summary>
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        int? GetUserIdFromPrincipal(ClaimsPrincipal principal);
        DateTime GetAccessTokenExpiration();
        DateTime GetRefreshTokenExpiration();
    }
}