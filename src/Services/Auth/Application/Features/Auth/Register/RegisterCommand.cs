using Auth.Application.Cqrs;
using Auth.Application.DTOs;

namespace Auth.Application.Features.Auth.Register
{
    /// <summary>
    /// Write intent to register a new user. Returns an <see cref="AuthResponseDto"/>
    /// with issued tokens on success, or Success=false + Message on failure.
    /// </summary>
    public sealed record RegisterCommand(
        string Username,
        string Email,
        string Password,
        string FirstName,
        string LastName) : ICommand<AuthResponseDto>;
}