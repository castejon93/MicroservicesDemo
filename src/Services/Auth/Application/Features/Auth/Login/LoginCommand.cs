using Auth.Application.Cqrs;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.Login
{
    /// <summary>
    /// Write intent to authenticate a user by email OR username + password.
    /// Returns an <see cref="AuthResponseDto"/> containing freshly issued access &amp;
    /// refresh tokens on success; Success=false with a Message on failure.
    /// </summary>
    public sealed record LoginCommand(
        string EmailOrUsername,
        string Password) : ICommand<AuthResponseDto>;
}
