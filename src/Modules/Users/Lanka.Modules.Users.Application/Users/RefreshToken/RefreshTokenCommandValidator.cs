using FluentValidation;

namespace Lanka.Modules.Users.Application.Users.RefreshToken;

internal sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        this.RuleFor(c => c.RefreshToken)
            .NotNull()
            .NotEmpty()
            .WithMessage("Refresh token is required");
    }
}
