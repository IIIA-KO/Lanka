using FluentValidation;
using Lanka.Modules.Users.Domain.Users.Emails;

namespace Lanka.Modules.Users.Application.Users.Login;

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        this.RuleFor(c => c.Email)
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(Email.MaxLength);

        this.RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(8)
            .WithMessage("Password length must be at least 8.")
            .MaximumLength(16)
            .WithMessage("Password length must not exceed 16.")
            .Matches(@"[A-Z]+")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]+")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]+")
            .WithMessage("Password must contain at least one number.")
            .Matches(@"[\!\?\*\.\$]+")
            .WithMessage("Password must contain at least one (!? *.$).");
    }
}
