using FluentValidation;

namespace Auth.Application.Features.Auth.RefreshToken
{
    /// <summary>
    /// Structural validation for <see cref="RefreshTokenCommand"/>.
    /// Token validity is verified in the handler via <c>ITokenService</c>.
    /// </summary>
    public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        /// <summary>
        /// Ensures both token strings are present before the handler performs
        /// cryptographic validation via <c>ITokenService</c>.
        /// </summary>
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty();
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}
