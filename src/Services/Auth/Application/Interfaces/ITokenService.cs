using Auth.Domain.Entities;
using System.Security.Claims;

namespace Auth.Application.Interfaces
{
    /// <summary>
    /// Abstraction for JWT access-token and refresh-token operations.
    /// Defined in Application; implemented in Infrastructure to keep the Application
    /// layer free of any JWT library dependency.
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a signed JWT access token whose claims are derived from
        /// <paramref name="user"/> (id, email, username, role, etc.).
        /// </summary>
        /// <param name="user">The authenticated user whose data populates the token claims.</param>
        /// <returns>A compact, URL-safe JWT string ready to be sent to the client.</returns>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Generates a cryptographically secure, opaque refresh token.
        /// The value is a Base64-encoded sequence of random bytes and carries no claims;
        /// its validity is checked by comparing it to the value stored on the <see cref="User"/> entity.
        /// </summary>
        /// <returns>A Base64 string to be persisted server-side and returned to the client.</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Validates the signature and well-formed claims of a JWT that may already be expired
        /// (lifetime validation is intentionally skipped here).
        /// Used during the token-refresh flow to extract the user identity from an expired
        /// access token before issuing a new token pair.
        /// </summary>
        /// <param name="token">The (possibly expired) JWT access token string.</param>
        /// <returns>
        /// A <see cref="ClaimsPrincipal"/> when the token's signature is valid and the algorithm
        /// matches; <see langword="null"/> when the token is malformed or uses an unexpected algorithm.
        /// </returns>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

        /// <summary>
        /// Extracts the numeric user ID from the <c>sub</c> / <c>NameIdentifier</c> claim
        /// inside <paramref name="principal"/>.
        /// </summary>
        /// <param name="principal">The claims principal obtained from a validated token.</param>
        /// <returns>The parsed user ID, or <see langword="null"/> if the claim is absent or non-numeric.</returns>
        int? GetUserIdFromPrincipal(ClaimsPrincipal principal);

        /// <summary>
        /// Returns the UTC <see cref="DateTime"/> at which a newly issued access token will expire.
        /// Computed as <c>UtcNow + ExpirationInMinutes</c> from <c>JwtSettings</c>.
        /// </summary>
        DateTime GetAccessTokenExpiration();

        /// <summary>
        /// Returns the UTC <see cref="DateTime"/> at which a newly issued refresh token will expire.
        /// Computed as <c>UtcNow + RefreshTokenExpirationInDays</c> from <c>JwtSettings</c>.
        /// </summary>
        DateTime GetRefreshTokenExpiration();
    }
}