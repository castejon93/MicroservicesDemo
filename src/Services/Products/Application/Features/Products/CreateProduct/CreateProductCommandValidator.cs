using FluentValidation;

namespace Products.Application.Features.Products.CreateProduct
{
    /// <summary>
    /// FluentValidation rules for <see cref="CreateProductCommand"/>. Runs inside the
    /// MediatR <c>ValidationBehavior</c> before the handler is invoked, ensuring the
    /// request satisfies structural invariants (non-empty name, non-negative money and
    /// stock) that the domain layer otherwise assumes.
    /// </summary>
    public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        /// <summary>
        /// Configures validation rules for the create-product command.
        /// </summary>
        public CreateProductCommandValidator()
        {
            // Name is the only required text field; 200 chars aligns with the EF column length.
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);

            // Money and stock must not be negative. Zero is permitted (free / out-of-stock products).
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Stock).GreaterThanOrEqualTo(0);
        }
    }
}