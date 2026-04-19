using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using MediatR;

namespace Auth.Application.Features.Auth.GetCurrentUser
{
    public sealed class GetCurrentUserQueryHandler(IUserRepository users)
        : IRequestHandler<GetCurrentUserQuery, UserInfo?>
    {
        public async Task<UserInfo?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await users.GetByIdAsync(request.UserId);
            if (user is null) return null;

            // Project entity → DTO (never leak domain entities across the API boundary).
            return new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role
            };
        }
    }
}