using Auth.Application.Cqrs;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.RefreshToken
{
    /// <summary>
    /// Exchanges a (possibly expired) access token + a valid refresh token for a
    /// new token pair. Handler validates the refresh token against the stored
    /// value + expiry on the <c>User</c> entity before issuing new tokens.
    /// </summary>
    public sealed record RefreshTokenCommand(
        string AccessToken,
        string RefreshToken) : ICommand<AuthResponseDto>;
}
