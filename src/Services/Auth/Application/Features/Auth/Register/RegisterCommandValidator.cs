using FluentValidation;

namespace Auth.Application.Features.Auth.Register
{
    /// <summary>
    /// Structural validation for <see cref="RegisterCommand"/>.
    /// Executed by the ValidationBehavior in the MediatR pipeline before the handler runs.
    /// </summary>
    public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        /// <summary>
        /// Configures structural field rules executed by <c>ValidationBehavior</c>
        /// before the handler runs. Business uniqueness checks (duplicate email /
        /// username) are handled in <see cref="RegisterCommandHandler"/>.
        /// </summary>
        public RegisterCommandValidator()
        {
            RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            // Minimum 8 characters enforced here; complexity rules live in the DTO annotations.
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        }
    }
}
