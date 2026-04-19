using Auth.Application.DTOs;
using Auth.Application.Features.Auth.GetCurrentUser;
using Auth.Application.Features.Auth.Login;
using Auth.Application.Features.Auth.Logout;
using Auth.Application.Features.Auth.RefreshToken;
using Auth.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Auth.WebAPI.Controllers
{
    /// <summary>
    /// Thin HTTP adapter. All business logic lives in MediatR handlers;
    /// this controller only maps HTTP in/out to commands/queries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public sealed class AuthController(ISender mediator) : ControllerBase
    {
        // POST api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Register(
            [FromBody] RegisterRequestDto dto,
            CancellationToken ct)
        {
            var result = await mediator.Send(
                new RegisterCommand(dto.Username, dto.Email, dto.Password, dto.FirstName, dto.LastName),
                ct);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        // POST api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login(
            [FromBody] LoginRequestDto dto,
            CancellationToken ct)
        {
            var result = await mediator.Send(new LoginCommand(dto.EmailOrUsername, dto.Password), ct);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        // POST api/auth/refresh
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Refresh(
            [FromBody] RefreshTokenRequestDto dto,
            CancellationToken ct)
        {
            var result = await mediator.Send(new RefreshTokenCommand(dto.AccessToken, dto.RefreshToken), ct);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        // POST api/auth/logout
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var ok = await mediator.Send(new LogoutCommand(userId.Value), ct);
            return ok ? NoContent() : NotFound();
        }

        // GET api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserInfo>> Me(CancellationToken ct)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var user = await mediator.Send(new GetCurrentUserQuery(userId.Value), ct);
            return user is null ? NotFound() : Ok(user);
        }

        // Extracts the authenticated user's id from the JWT "sub"/NameIdentifier claim.
        private int? GetUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.FindFirstValue("sub");
            return int.TryParse(raw, out var id) ? id : null;
        }
    }
}