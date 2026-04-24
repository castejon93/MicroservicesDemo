using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using MediatR;

namespace Auth.Application.Features.Auth.GetCurrentUser
{
    /// <summary>
    /// Handles <see cref="GetCurrentUserQuery"/>: loads the <see cref="Auth.Domain.Entities.User"/>
    /// entity by ID and projects it to a <see cref="UserInfo"/> DTO.
    /// </summary>
    public sealed class GetCurrentUserQueryHandler(IUserRepository users)
        : IRequestHandler<GetCurrentUserQuery, UserInfo?>
    {
        /// <summary>
        /// Retrieves and projects the current user's profile.
        /// </summary>
        /// <returns>
        /// A <see cref="UserInfo"/> DTO when the user exists;
        /// <see langword="null"/> when the ID is not found.
        /// </returns>
        public async Task<UserInfo?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await users.GetByIdAsync(request.UserId);
            if (user is null) return null;

            // Project the domain entity to a DTO — domain entities must never cross the API boundary.
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